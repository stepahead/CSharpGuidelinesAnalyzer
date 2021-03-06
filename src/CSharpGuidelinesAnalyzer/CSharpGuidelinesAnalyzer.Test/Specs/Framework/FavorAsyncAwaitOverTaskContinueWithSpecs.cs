using System;
using System.Threading.Tasks;
using CSharpGuidelinesAnalyzer.Rules.Framework;
using CSharpGuidelinesAnalyzer.Test.TestDataBuilders;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CSharpGuidelinesAnalyzer.Test.Specs.Framework
{
    public sealed class FavorAsyncAwaitOverTaskContinueWithSpecs : CSharpGuidelinesAnalysisTestFixture
    {
        protected override string DiagnosticId => FavorAsyncAwaitOverTaskContinueWithAnalyzer.DiagnosticId;

        [Fact]
        internal void When_method_contains_invocation_of_TaskContinueWith_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(Task).Namespace)
                .InGlobalScope(@"
                    namespace N
                    {
                        class C
                        {
                            Task<int> M(int i)
                            {
                                return [|Task.Delay(1).ContinueWith(t => i)|];
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The call to 'Task.ContinueWith' in 'C.M(int)' should be replaced with an async method.");
        }

        [Fact]
        internal void When_method_contains_invocation_of_TaskContinueWith_with_using_static_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(Task).Namespace)
                .InGlobalScope(@"
                    using static System.Threading.Tasks.Task;

                    namespace N
                    {
                        class C
                        {
                            Task<int> M(int i)
                            {
                                return [|Delay(1).ContinueWith(t => i)|];
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source,
                "The call to 'Task.ContinueWith' in 'C.M(int)' should be replaced with an async method.");
        }

        [Fact]
        internal void When_method_contains_invocation_of_TaskContinueWith_in_alternate_namespace_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(NotImplementedException).Namespace)
                .InGlobalScope(@"
                    namespace N
                    {
                        public class Task<T>
                        {
                        }

                        public class Task
                        {
                            public Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction)
                            {
                                throw new NotImplementedException();
                            }

                            public static Task Delay(int millisecondsDelay)
                            {
                                throw new NotImplementedException();
                            }
                        }

                        class C
                        {
                            Task<int> M(int i)
                            {
                                return Task.Delay(1).ContinueWith(t => i);
                            }
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyGuidelineDiagnostic(source);
        }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new FavorAsyncAwaitOverTaskContinueWithAnalyzer();
        }
    }
}
