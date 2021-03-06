﻿using System;
using System.Collections.Generic;
using System.Linq;
using Jabberwocky.Glass.Factory.Caching;
using Jabberwocky.Glass.Factory.Implementation;
using Jabberwocky.Glass.Models;

namespace Jabberwocky.Glass.Factory
{
	public class GlassInterfaceFactory : IGlassInterfaceFactory
	{
		private readonly IGlassTemplateCacheService _templateCache;
		private readonly IImplementationFactory _factory;

		/// <summary>
		/// Gets the template cache service used by this instance
		/// </summary>
		internal IGlassTemplateCacheService TemplateCacheService => _templateCache;

		public GlassInterfaceFactory(IGlassTemplateCacheService templateCache, IImplementationFactory factory)
		{
			if (templateCache == null) throw new ArgumentNullException(nameof(templateCache));
			if (factory == null) throw new ArgumentNullException(nameof(factory));

			_templateCache = templateCache;
			_factory = factory;
		}

		public T GetItem<T>(IGlassBase model) where T : class
		{
			var implType = GetItemFromInterface(model, typeof(T));

			return implType != null
				? _factory.Create<T, IGlassBase>(implType, model)
				: default(T);
		}

		public IEnumerable<T> GetItems<T>(IEnumerable<IGlassBase> models) where T : class
		{
			if (models == null) return Enumerable.Empty<T>();

			return models.Select(GetItem<T>).Where(x => x != default(T));
		}

		private Type GetItemFromInterface(IGlassBase item, Type interfaceType)   // interfaceType: this will be non-generic type, ie. IListable
		{
			if (item == null || interfaceType == null) return null;

			return _templateCache.GetImplementingTypeForItem(item, interfaceType);
		}
	}
}
