using ActionList.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ActionList.Filters {
    public class ValidateModelFilter : ActionFilterAttribute {
        public override void OnActionExecuting(ActionExecutingContext context) {
            if (!context.ModelState.IsValid) {
                var errors = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                var errorResponse = new ErrorResponse {
                    IsError = true,
                    Error = new ErrorDetail {
                        Code = "400001",
                        Message = "Validation failed.",
                        Details = errors
                    }
                };

                context.Result = new BadRequestObjectResult(errorResponse);
            }
        }
    }
}
