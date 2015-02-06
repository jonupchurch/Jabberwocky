﻿using System;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Glass.Mapper.Sc;
using Glass.Mapper.Sc.Configuration.Attributes;
using Jabberwocky.Glass.Factory.Attributes;
using Jabberwocky.Glass.Factory.Implementation;
using Jabberwocky.Glass.Factory.Util;

namespace Jabberwocky.Glass.Factory.Interceptors
{
	public class FallbackInterceptor : IInterceptor
	{
		private readonly Type _interfaceType;
		private readonly object _model;

		private readonly IGlassTemplateCache _templateCache;
		private readonly IImplementationFactory _implementationFactory;
		private readonly ISitecoreService _service;

		public FallbackInterceptor(Type interfaceType, object model, IGlassTemplateCache templateCache, IImplementationFactory implementationFactory, ISitecoreService service)
		{
			if (interfaceType == null) throw new ArgumentNullException("interfaceType");
			if (model == null) throw new ArgumentNullException("model");
			if (templateCache == null) throw new ArgumentNullException("templateCache");
			if (implementationFactory == null) throw new ArgumentNullException("implementationFactory");
			if (service == null) throw new ArgumentNullException("service");
			_interfaceType = interfaceType;
			_model = model;
			_templateCache = templateCache;
			_implementationFactory = implementationFactory;
			_service = service;
		}

		public void Intercept(IInvocation invocation)
		{
			//var glassFactoryTypeAttribute = invocation.TargetType.GetCustomAttribute<GlassFactoryTypeAttribute>();
			var glassFactoryTypeAttribute = invocation.TargetType.GetCustomAttributes(typeof(GlassFactoryTypeAttribute), true).Cast<GlassFactoryTypeAttribute>().FirstOrDefault();
			if (invocation.MethodInvocationTarget.IsAbstract && glassFactoryTypeAttribute != null)
			{
				// Custom resolution required
				ResolveInvocation(invocation, glassFactoryTypeAttribute);
			}
			else
			{
				// Otherwise proceed as normal... This should really only happen if the method is virtual
				try
				{
					invocation.Proceed();
				}
				catch (NotImplementedException)
				{
					ResolveInvocation(invocation, glassFactoryTypeAttribute);
				}
			}

			// In case of value types...
			var returnType = invocation.MethodInvocationTarget.ReturnType;
			if (invocation.ReturnValue == null && returnType.IsValueType)
			{
				invocation.ReturnValue = Activator.CreateInstance(returnType);
			}
		}

		private void ResolveInvocation(IInvocation invocation, GlassFactoryTypeAttribute glassFactoryTypeAttribute)
		{
			var glassType = glassFactoryTypeAttribute.Type;
			//var sitecoreAttribute = glassType.GetCustomAttribute<SitecoreTypeAttribute>();
			var sitecoreAttribute = glassType.GetCustomAttributes(typeof(SitecoreTypeAttribute), true).Cast<SitecoreTypeAttribute>().FirstOrDefault();

			// If this is a final fallback type that is not associated with a direct Sitecore template
			if (sitecoreAttribute == null) return;

			var templateId = sitecoreAttribute.TemplateId;

			var templateItem = _service.GetItem<IBaseTemplates>(new Guid(templateId));
			var templateMapping = _templateCache.TemplateCache[_interfaceType];

			var matchingId = _templateCache.GetBaseTemplates(templateItem, _service)
				.Select(guid => guid.ToString())
				.FirstOrDefault(templateMapping.ContainsKey);

			// This can happen if we fail to find an eligible base template with a corresponding implementation
			// In this case, don't do anything -> invocation maps to default value
			if (matchingId == null) return;

			var matchingType = templateMapping[matchingId];

			var fallbackImpl = _implementationFactory.Create(matchingType, _interfaceType, _model);
			var invocationTarget = invocation.MethodInvocationTarget;
			var targetMethod = invocationTarget.IsGenericMethod
				? invocationTarget.GetGenericMethodDefinition()
				: invocationTarget;

			var map = invocation.TargetType.GetInterfaceMap(_interfaceType);
			var index = Array.IndexOf(map.TargetMethods, targetMethod);

			if (index == -1) return;

			MethodInfo interfaceMethod = map.InterfaceMethods[index];
			interfaceMethod = interfaceMethod.IsGenericMethod
				? interfaceMethod.MakeGenericMethod(invocationTarget.GetGenericArguments())
				: interfaceMethod;

			invocation.ReturnValue = interfaceMethod.Invoke(fallbackImpl, invocation.Arguments);
		}
	}
}
