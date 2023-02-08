using AspNetCore.Simple.Sdk.AutoDependencyRegistration;

namespace AspNetCore.Simple.Sdk.Api.WeatherForecast.V1
{
    public static class AddWeatherForecastExtension
    {
        public static void AddWeatherForecast(this AutoRegistration autoRegistration)
        {
            autoRegistration.DoAutoRegistrationFor<WeatherForecastController>();
        }
    }
}
