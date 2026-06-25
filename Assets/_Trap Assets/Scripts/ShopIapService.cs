using System;
using UnityEngine;

#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

public class ShopIapService : MonoBehaviour
#if UNITY_PURCHASING
    , IStoreListener
#endif
{
    [SerializeField] private bool simulateEditorPurchasesWhenIapUnavailable = true;

    private ShopCatalog catalog;
    private Action<string> onPurchaseSucceeded;

#if UNITY_PURCHASING
    private IStoreController storeController;
    private IExtensionProvider extensionProvider;
    private bool initializationStarted;
#endif

    public void Initialize(ShopCatalog shopCatalog, Action<string> purchaseSucceeded)
    {
        catalog = shopCatalog;
        onPurchaseSucceeded = purchaseSucceeded;

#if UNITY_PURCHASING
        if (storeController != null || initializationStarted || catalog == null)
        {
            return;
        }

        ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        ShopItemDefinition[] items = catalog.Items;
        for (int i = 0; i < items.Length; i++)
        {
            ShopItemDefinition item = items[i];
            if (item == null || !item.isEnabled || !item.IsIap || string.IsNullOrWhiteSpace(item.productId))
            {
                continue;
            }

            builder.AddProduct(item.productId, ToUnityProductType(item.iapProductKind));
        }

        initializationStarted = true;
        UnityPurchasing.Initialize(this, builder);
#else
        Debug.LogWarning("Unity IAP is not available yet. Add/resolve com.unity.purchasing to enable real-money purchases.");
#endif
    }

    public bool Purchase(ShopItemDefinition item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.productId))
        {
            return false;
        }

#if UNITY_PURCHASING
        if (storeController == null)
        {
            Debug.LogWarning($"Unity IAP is not initialized. Purchase skipped for product: {item.productId}");
            return false;
        }

        Product product = storeController.products.WithID(item.productId);
        if (product == null || !product.availableToPurchase)
        {
            Debug.LogWarning($"Unity IAP product is unavailable: {item.productId}");
            return false;
        }

        storeController.InitiatePurchase(product);
        return true;
#else
        #if UNITY_EDITOR
        if (simulateEditorPurchasesWhenIapUnavailable)
        {
            Debug.Log($"Simulating editor IAP purchase because Unity IAP is not resolved: {item.productId}");
            onPurchaseSucceeded?.Invoke(item.productId);
            return true;
        }
        #endif

        Debug.LogWarning($"Unity IAP package is not resolved. Purchase skipped for product: {item.productId}");
        return false;
#endif
    }

#if UNITY_PURCHASING
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        extensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogWarning($"Unity IAP initialization failed: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogWarning($"Unity IAP initialization failed: {error} {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string productId = args != null && args.purchasedProduct != null
            ? args.purchasedProduct.definition.id
            : string.Empty;

        if (!string.IsNullOrWhiteSpace(productId))
        {
            onPurchaseSucceeded?.Invoke(productId);
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        string productId = product != null && product.definition != null ? product.definition.id : "unknown";
        Debug.LogWarning($"Unity IAP purchase failed for {productId}: {failureReason}");
    }

    private static ProductType ToUnityProductType(ShopIapProductKind productKind)
    {
        return productKind == ShopIapProductKind.NonConsumable
            ? ProductType.NonConsumable
            : ProductType.Consumable;
    }
#endif
}
