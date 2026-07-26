using Microsoft.Playwright;
using PlaywrightLibrary.Components;

namespace Toolshop.Tests.Pages;

/// <summary>
/// Page object for the Toolshop checkout stepper (<c>/checkout</c>):
/// cart → sign-in → address → payment → confirmation. Composes the library's
/// <see cref="SelectComponent"/> for the country and payment dropdowns.
/// </summary>
public sealed class CheckoutPage(IPage page) : IPageLevelComponent<CheckoutPage>
{
    public static CheckoutPage Create(IPage page) => new(page);

    private SelectComponent Country => new(page.Locator("[data-test='country']"));
    private SelectComponent PaymentMethod => new(page.Locator("[data-test='payment-method']"));

    /// <summary>The banner shown once the order is placed.</summary>
    public ILocator SuccessMessage => page.Locator("[data-test='payment-success-message']");

    public Task ProceedToCheckoutAsync() =>
        new ButtonComponent(page.Locator("[data-test='proceed-1']")).ClickAsync();

    public Task ContinueAsSignedInAsync() =>
        new ButtonComponent(page.Locator("[data-test='proceed-2']")).ClickAsync();

    public Task ProceedToPaymentAsync() =>
        new ButtonComponent(page.Locator("[data-test='proceed-3']")).ClickAsync();

    public Task ConfirmOrderAsync() =>
        new ButtonComponent(page.Locator("[data-test='finish']")).ClickAsync();

    /// <summary>Fills the billing address. <paramref name="countryCode"/> is an ISO code (e.g. "US").</summary>
    public async Task FillAddressAsync(string countryCode, string postcode, string street, string city, string state)
    {
        await Country.SelectByValueAsync(countryCode);
        await page.Locator("[data-test='postal_code']").FillAsync(postcode);
        await page.Locator("[data-test='house_number']").FillAsync("1");
        await page.Locator("[data-test='street']").FillAsync(street);
        await page.Locator("[data-test='city']").FillAsync(city);
        await page.Locator("[data-test='state']").FillAsync(state);
    }

    public async Task PayByBankTransferAsync(string bankName, string accountName, string accountNumber)
    {
        await PaymentMethod.SelectByValueAsync("bank-transfer");
        await page.Locator("[data-test='bank_name']").FillAsync(bankName);
        await page.Locator("[data-test='account_name']").FillAsync(accountName);
        await page.Locator("[data-test='account_number']").FillAsync(accountNumber);
    }
}
