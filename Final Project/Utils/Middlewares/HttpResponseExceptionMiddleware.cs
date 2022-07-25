using Microsoft.AspNetCore.Diagnostics;
using MongoDB.Driver;
using System.Net;
using Final_Project.Utils.Resources.Commons;
using Final_Project.Utils.Resources.Exceptions;

namespace Final_Project.Utils.Middlewares
{
    public static class HttpResponseExceptionMiddleware
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        Exception RootCause;
                        if (contextFeature.Error is AggregateException)
                        {
                            RootCause = (contextFeature.Error as AggregateException).InnerException;
                        }
                        else
                        {
                            RootCause = contextFeature.Error;
                        }
                        switch (RootCause)
                        {
                            case HttpReturnException:
                                {
                                    HttpReturnException Exception = RootCause as HttpReturnException;
                                    context.Response.StatusCode = (int)Exception.Status;
                                    string Message = Exception.Message;
                                    await context.Response.WriteAsync(new ErrorDetail()
                                    {
                                        StatusCode = context.Response.StatusCode,
                                        Message = Message
                                    }.ToString());
                                    break;
                                }
                            case MongoWriteException:
                                {
                                    MongoWriteException Exception = RootCause as MongoWriteException;
                                    context.Response.StatusCode = 400;
                                    string Message = Exception.WriteError.Message;
                                    Console.WriteLine(Message);
                                    if (Exception.WriteError.Code == 11000)
                                    {
                                        Message = Message.Split("dup key: ")[1].Replace("\"", "'").Replace("{ ", "").Replace("}", "");
                                        Message += "is already exist";
                                    }
                                    await context.Response.WriteAsync(new ErrorDetail()
                                    {
                                        StatusCode = context.Response.StatusCode,
                                        Message = Message
                                    }.ToString());
                                    break;
                                }
                            default:
                                {
                                    await context.Response.WriteAsync(new ErrorDetail()
                                    {
                                        StatusCode = context.Response.StatusCode,
                                        Message = "Internal server error"
                                    }.ToString());
                                    break;
                                }
                        }
                    }
                });
            });
        }
    }
}