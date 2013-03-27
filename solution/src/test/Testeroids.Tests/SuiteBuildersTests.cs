﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SuiteBuildersTests.cs" company="Testeroids">
//   © 2013 Testeroids. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Testeroids.Tests
{
    using System;

    using Moq;

    using NUnit.Framework;

    using Testeroids.Aspects.Attributes;

    public class SuiteBuildersTests
    {
        public abstract class given_instantiated_Sut : ContextSpecification<Test>
        {
            #region Context

            protected IMock<ICalculator> InjectedCalculatorMock { get; private set; }

            protected override void EstablishContext()
            {
                base.EstablishContext();

                this.InjectedCalculatorMock = this.MockRepository.CreateMock<ICalculator>();
            }

            protected override Test CreateSubjectUnderTest()
            {
                return new Test(this.InjectedCalculatorMock.Object);
            }

            #endregion          
            
            /// <summary>
            /// TODO: This class has 2 test fixtures which require a simpler triangulation mechanism.
            /// </summary>
            [AbstractTestFixture]
            public abstract class when_Sum_is_called : given_instantiated_Sut
            {
                #region Context

                private int Result { get; set; }

                private int ReturnedSum { get; set; }

                private int SpecifiedOperand1 { get; set; }

                private int SpecifiedOperand2 { get; set; }

                protected abstract int EstablishSpecifiedOperand1();

                protected abstract int EstablishSpecifiedOperand2();

                protected abstract int EstablishReturnedSum();

                protected override void EstablishContext()
                {
                    base.EstablishContext();

                    this.SpecifiedOperand1 = this.EstablishSpecifiedOperand1();
                    this.SpecifiedOperand2 = this.EstablishSpecifiedOperand2();

                    this.ReturnedSum = this.EstablishReturnedSum();

                    this.InjectedCalculatorMock
                        .Setup(o => o.Sum(It.IsAny<int>(), It.IsAny<int>()))
                        .Returns(this.ReturnedSum)
                        .Verifiable();
                }

                protected override sealed void Because()
                {
                    this.Result = this.Sut.Sum(this.SpecifiedOperand1, this.SpecifiedOperand2);
                }

                #endregion

                [TestFixture]
                public class with_SpecifiedOperand1_equal_to_10_and_SpecifiedOperand2_equal_to_7 : when_Sum_is_called
                {
                    #region Context

                    protected override int EstablishSpecifiedOperand1()
                    {
                        return 10;
                    }

                    protected override int EstablishSpecifiedOperand2()
                    {
                        return 7;
                    }

                    protected override int EstablishReturnedSum()
                    {
                        // Return an erroneous value, just to certify that we are returning the value which is handed out by the mock
                        return int.MaxValue;
                    }

                    #endregion
                }

                [TestFixture]
                public class with_SpecifiedOperand1_equal_to_10_and_SpecifiedOperand2_equal_to_minus_7 : when_Sum_is_called
                {
                    #region Context

                    protected override int EstablishSpecifiedOperand1()
                    {
                        return 10;
                    }

                    protected override int EstablishSpecifiedOperand2()
                    {
                        return -7;
                    }

                    protected override int EstablishReturnedSum()
                    {
                        return 3;
                    }

                    #endregion
                }

                [Test]
                public void then_Sum_is_called_once_on_InjectedTestMock()
                {
                    this.InjectedCalculatorMock.Verify(o => o.Sum(It.IsAny<int>(), It.IsAny<int>()), Times.Once());
                }

                [Test]
                public void then_Sum_is_called_once_on_InjectedTestMock_passing_SpecifiedOperand1()
                {
                    this.InjectedCalculatorMock.Verify(o => o.Sum(this.SpecifiedOperand1, It.IsAny<int>()), Times.Once());
                }

                [Test]
                public void then_Sum_is_called_once_on_InjectedTestMock_passing_SpecifiedOperand2()
                {
                    this.InjectedCalculatorMock.Verify(o => o.Sum(It.IsAny<int>(), this.SpecifiedOperand2), Times.Once());
                }

                [Test]
                public void then_Result_matches_ReturnedSum()
                {
                    Assert.AreEqual(this.ReturnedSum, this.Result);
                }
            }
        }
    }
}