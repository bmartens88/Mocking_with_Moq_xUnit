using Moq;
using Xunit;

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationsEvaluatorShould
    {
        [Fact]
        public void AcceptHighIncomeApplications()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator = new();

            CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new() { GrossAnnualIncome = 100_000 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }

        [Fact]
        public void ReferYoungApplications()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator = new();

            CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new() { Age = 19 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void DeclineLowIncomeApplications()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator = new();

            mockValidator.Setup(x => x.IsValid("x")).Returns(true);

            CreditCardApplicationEvaluator sut = new(mockValidator.Object);

            CreditCardApplication application = new()
            {
                GrossAnnualIncome = 19_999,
                Age = 42,
                FrequentFlyerNumber = "x"
            };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }
    }
}
