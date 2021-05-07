namespace CreditCardApplications
{
    public class FraudLookup
    {
        public bool IsFraudRisk(CreditCardApplication application) =>
            CheckApplication(application);

        virtual protected bool CheckApplication(CreditCardApplication application) =>
            application.LastName == "Smith";
    }
}
