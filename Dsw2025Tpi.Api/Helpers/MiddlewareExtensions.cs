namespace Dsw2025Tpi.Api.Helpers
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomMiddlewares(this IApplicationBuilder app)
        {
            app.UseExceptionMiddleware();
            app.UseHttpsRedirection();
            app.UseCors("PermitirFrontend");
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }
    }
}
