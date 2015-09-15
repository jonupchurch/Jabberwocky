﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using StackExchange.Profiling;
using Profiler = StackExchange.Profiling.MiniProfiler;

namespace Jabberwocky.Autofac.Extras.MiniProfiler.Interceptors
{
	public class AsyncProfilingInterceptor : IInterceptor
	{
		private const string UnknownType = "UNKNOWNTYPE";

		public void Intercept(IInvocation invocation)
		{
			var profiler = Profiler.Current;
			if (profiler == null)
			{
				invocation.Proceed();
				return;
			}

			var returnType = invocation.Method.ReturnType;
			var typeName = invocation.TargetType?.Name ?? invocation.Proxy?.GetType().FullName ?? UnknownType;

			// We only care about Tasks (Task or Task<>)... note that this is NOT aware of other async-aware members (ie. *Awaiter)
			if (returnType == typeof(Task) || (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>)))
			{
				// If we get here, we're async
				var timing = profiler.Step(typeName + ":" + invocation.Method.Name) as Timing;

				// start time
				var stopWatch = new Stopwatch();
				stopWatch.Start();

				invocation.Proceed();

				// end time
				stopWatch.Stop();
				var timeForSyncInvocation = stopWatch.ElapsedMilliseconds;

				var returnValue = invocation.ReturnValue;

				var task = (Task)returnValue;
				task.ContinueWith(t =>
				{
					// This method corrects the total duration offsets by appending the asynchronous timing information to the parent context
					if (timing != null)
					{
						timing.Stop();
						var asyncInvocationTime = timing.DurationMilliseconds - (decimal)timeForSyncInvocation;
						timing.ParentTiming.DurationMilliseconds += asyncInvocationTime;

						// Dispose of timing info
						var disposable = timing as IDisposable;
						disposable.Dispose();
					}
				}, TaskContinuationOptions.ExecuteSynchronously);
			}
			else
			{
				// We're a synchronous operation, so just proceed
				using (profiler.Step(typeName + ":" + invocation.Method.Name))
				{
					invocation.Proceed();
				}
			}
		}
	}
}
