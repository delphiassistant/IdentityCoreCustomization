using Microsoft.AspNetCore.Mvc.ModelBinding;

using System.Globalization;
using System.Reflection;

namespace IdentityCoreCustomization.Classes.ModelBinding
{
    public class PersianDateModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // Default formats
            string?[] formats = { "yyyy/MM/dd HH:mm", "yyyy/MM/dd" };

            // Check for DisplayFormat attribute
            var metadata = bindingContext.ModelMetadata;
            var displayFormat = metadata.ContainerType
                ?.GetProperty(metadata.PropertyName)
                ?.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayFormatAttribute>();

            if (displayFormat != null && displayFormat.ApplyFormatInEditMode)
            {
                if (!formats.Contains(displayFormat.DataFormatString))
                {
                    formats = formats.Append(displayFormat.DataFormatString).ToArray();
                }
            }

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            DateTime parsedDate;
            if (DateTime.TryParseExact(value, formats, new CultureInfo("fa-IR"), DateTimeStyles.None, out parsedDate))
            {
                // Convert to Persian calendar date
                var persianCalendar = new PersianCalendar();
                var year = persianCalendar.GetYear(parsedDate);
                var month = persianCalendar.GetMonth(parsedDate);
                var day = persianCalendar.GetDayOfMonth(parsedDate);
                var hour = persianCalendar.GetHour(parsedDate);
                var minute = persianCalendar.GetMinute(parsedDate);
                var second = persianCalendar.GetSecond(parsedDate);
                DateTime resultDate = persianCalendar.ToDateTime(year, month, day, hour, minute, second, 0);

                bindingContext.Result = ModelBindingResult.Success(resultDate);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Invalid date format.");
            }

            return Task.CompletedTask;
        }
    }


}
