using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Documentation
{
    public class Specifier<T> : ISpecifier
    {
        public string GetApiDescription()
        {
            var attribute = typeof(T)
                                    .GetCustomAttributes()
                                    .OfType<ApiDescriptionAttribute>()
                                    .FirstOrDefault();
            return attribute != null ? attribute.Description : null;
        }

        public string[] GetApiMethodNames()
        {
            return typeof(T).GetMethods()
                            .Where(u => u.GetCustomAttribute<ApiMethodAttribute>() != null)
                            .Select(u => u.Name)
                            .OrderBy(u => u)
                            .ToArray();
        }

        public string GetApiMethodDescription(string methodName)
        {
            var method = GetApiMethod(methodName);
            var attribute = method != null ? method.GetCustomAttribute<ApiDescriptionAttribute>() : null;
            return attribute != null ? attribute.Description : null;
        }

        public string[] GetApiMethodParamNames(string methodName)
        {
            var method = GetApiMethod(methodName);
            return method.GetParameters().Select(u => u.Name).ToArray();
        }

        public string GetApiMethodParamDescription(string methodName, string paramName)
        {
            var method = GetApiMethod(methodName);
            var param = GetParametr(method, paramName);
            return GetParametrDescription(param);
        }

        public ApiParamDescription GetApiMethodParamFullDescription(string methodName, string paramName)
        {
            var method = GetApiMethod(methodName);
            var param = GetParametr(method, paramName);
            var paramDescription = new ApiParamDescription();
            paramDescription.ParamDescription = new CommonDescription(paramName,
                                                            GetParametrDescription(param));
            if (param != null)
                DetermineApiParamDescription(param.GetCustomAttributes(), paramDescription);
            return paramDescription;
        }

        public ApiMethodDescription GetApiMethodFullDescription(string methodName)
        {
            var method = GetApiMethod(methodName);
            if (method == null) return null;
            var methodDescription = new ApiMethodDescription();
            methodDescription.MethodDescription = new CommonDescription(methodName,
                                                    GetApiMethodDescription(methodName));
            methodDescription.ParamDescriptions = method.GetParameters()
                .Select(u => GetApiMethodParamFullDescription(methodName, u.Name))
                .ToArray();
            var returnAttributes = method.ReturnParameter.GetCustomAttributes();
            if (returnAttributes.Count() != 0)
            {
                methodDescription.ReturnDescription = new ApiParamDescription();
                methodDescription.ReturnDescription.ParamDescription = new CommonDescription();
                DetermineApiParamDescription(returnAttributes, methodDescription.ReturnDescription);
            }
            return methodDescription;
        }

        private MethodInfo GetApiMethod(string methodName)
        {
            var method = typeof(T).GetMethod(methodName);
            if (method == null || method.GetCustomAttribute<ApiMethodAttribute>() == null) return null;
            return method;
        }

        private ParameterInfo GetParametr(MethodInfo method, string paramName)
        {
            return method != null ? method.GetParameters()
                                        .Where(u => u.Name == paramName)
                                        .FirstOrDefault() : null;
        }

        private string GetParametrDescription(ParameterInfo param)
        {
            if (param == null || param.GetCustomAttribute<ApiDescriptionAttribute>() == null)
                return null;
            return param.GetCustomAttribute<ApiDescriptionAttribute>().Description;
        }

        private void DetermineApiParamDescription(IEnumerable<Attribute> attributes,
                                                            ApiParamDescription description)
        {
            if (attributes != null)
            {
                var paramValidation = attributes.OfType<ApiIntValidationAttribute>().FirstOrDefault();
                description.MinValue = paramValidation != null ? paramValidation.MinValue : null;
                description.MaxValue = paramValidation != null ? paramValidation.MaxValue : null;
                var attributeRequired = attributes.OfType<ApiRequiredAttribute>().FirstOrDefault();
                description.Required = attributeRequired != null && attributeRequired.Required;
            }
            else
                description.Required = false;
        }
    }
}

