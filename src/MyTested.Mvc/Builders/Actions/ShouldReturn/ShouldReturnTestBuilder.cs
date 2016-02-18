﻿namespace MyTested.Mvc.Builders.Actions.ShouldReturn
{
    using System;
    using System.Reflection;
    using Base;
    using Contracts.Actions;
    using Exceptions;
    using Utilities.Extensions;
    using Utilities;
    using Utilities.Validators;
    using Internal.TestContexts;

    /// <summary>
    /// Used for testing returned action result.
    /// </summary>
    /// <typeparam name="TActionResult">Result from invoked action in ASP.NET MVC controller.</typeparam>
    public partial class ShouldReturnTestBuilder<TActionResult>
        : BaseTestBuilderWithActionResult<TActionResult>, IShouldReturnTestBuilder<TActionResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShouldReturnTestBuilder{TActionResult}" /> class.
        /// </summary>
        /// <param name="controller">Controller on which the action will be tested.</param>
        /// <param name="actionName">Name of the tested action.</param>
        /// <param name="caughtException">Caught exception during the action execution.</param>
        /// <param name="actionResult">Result from the tested action.</param>
        public ShouldReturnTestBuilder(ControllerTestContext testContext)
            : base(testContext)
        {
        }

        private void ValidateActionReturnType(Type typeOfExpectedReturnValue, bool canBeAssignable = false, bool allowDifferentGenericTypeDefinitions = false)
        {
            CommonValidator.CheckForException(this.CaughtException);

            var typeInfoOfExpectedReturnValue = typeOfExpectedReturnValue.GetTypeInfo();

            var typeOfActionResult = this.ActionResult.GetType();
            var typeInfoOfActionResult = typeOfActionResult.GetTypeInfo();

            var isAssignableCheck = canBeAssignable && Reflection.AreNotAssignable(typeOfExpectedReturnValue, typeOfActionResult);
            if (isAssignableCheck && allowDifferentGenericTypeDefinitions
                && Reflection.IsGeneric(typeOfExpectedReturnValue) && Reflection.IsGenericTypeDefinition(typeOfExpectedReturnValue))
            {
                isAssignableCheck = Reflection.AreAssignableByGeneric(typeOfExpectedReturnValue, typeOfActionResult);

                if (!isAssignableCheck && !typeInfoOfActionResult.IsGenericType)
                {
                    isAssignableCheck = true;
                }
                else
                {
                    isAssignableCheck =
                        !Reflection.ContainsGenericTypeDefinitionInterface(typeOfExpectedReturnValue, typeOfActionResult);
                }
            }

            var strictlyEqualCheck = !canBeAssignable && Reflection.AreDifferentTypes(typeOfExpectedReturnValue, typeOfActionResult);

            var invalid = isAssignableCheck || strictlyEqualCheck;
            if (invalid && typeInfoOfExpectedReturnValue.IsGenericTypeDefinition && typeInfoOfActionResult.IsGenericType)
            {
                var actionResultGenericDefinition = typeInfoOfActionResult.GetGenericTypeDefinition();
                if (actionResultGenericDefinition == typeOfExpectedReturnValue)
                {
                    invalid = false;
                }
            }

            if (invalid && typeInfoOfExpectedReturnValue.IsGenericType && typeInfoOfActionResult.IsGenericType)
            {
                invalid = !Reflection.AreAssignableByGeneric(typeOfExpectedReturnValue, typeOfActionResult);
            }

            if (invalid)
            {
                this.ThrowNewGenericHttpActionResultAssertionException(
                    typeOfExpectedReturnValue.ToFriendlyTypeName(),
                    typeOfActionResult.ToFriendlyTypeName());
            }
        }

        private void ValidateActionReturnType<TExpectedType>(bool canBeAssignable = false, bool allowDifferentGenericTypeDefinitions = false)
        {
            this.ValidateActionReturnType(typeof(TExpectedType), canBeAssignable, allowDifferentGenericTypeDefinitions);
        }

        private void ThrowNewGenericHttpActionResultAssertionException(
            string typeNameOfExpectedReturnValue,
            string typeNameOfActionResult)
        {
            throw new ActionResultAssertionException(string.Format(
                    "When calling {0} action in {1} expected action result to be {2}, but instead received {3}.",
                    this.ActionName,
                    this.Controller.GetName(),
                    typeNameOfExpectedReturnValue,
                    typeNameOfActionResult));
        }
    }
}
