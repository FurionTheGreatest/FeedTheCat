using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.DefaultLayers;
using UnityEngine.GameFoundation.DefaultLayers.Persistence;
using UnityEngine.Promise;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene for showcasing the wallet and store sample.
    /// </summary>
    public class Wallet : MonoBehaviour
    {
        /// <summary>
        /// Flag for whether the Wallet has changes in it that have not yet been updated in the UI.
        /// </summary>
        private bool m_WalletChanged;

        /// <summary>
        /// We will want to hold onto reference to currency for easy use later.
        /// </summary>
        private Currency m_softCurrencyDefinition;
        
        private Currency m_hardCurrencyDefinition;

        /// <summary>
        /// Used to reduce times mainText.text is accessed.
        /// </summary>
        private readonly StringBuilder m_softCurrencyDisplayText = new StringBuilder();
        private readonly StringBuilder m_hardCurrencyDisplayText = new StringBuilder();

        /// <summary>
        /// We will need a reference to the main text box in the scene so we can easily modify it.
        /// </summary>
        public TMP_Text softCurrencyText;
        
        /// <summary>
        /// We will need a reference to the main text box in the scene so we can easily modify it.
        /// </summary>
        public TMP_Text hardCurrencyText;

        private PersistenceDataLayer m_DataLayer;
        
        private void Start()
        {
            // - Initialize must always be called before working with any game foundation code.
            // - GameFoundation requires an IDataAccessLayer object that will provide and persist
            //   the data required for the various services (Inventory, Wallet, ...).
            // - For this sample we don't need to persist any data so we use the MemoryDataLayer
            //   that will store GameFoundation's data only for the play session.
            m_DataLayer = new PersistenceDataLayer(
                new LocalPersistence("DataPersistence", new JsonDataSerializer()));
            
            GameFoundation.Initialize(m_DataLayer, OnGameFoundationInitialized, OnInitFailed);

            m_softCurrencyDefinition = GameFoundation.catalogs.currencyCatalog.FindItem("coin");
            m_hardCurrencyDefinition = GameFoundation.catalogs.currencyCatalog.FindItem("gem");

            // Here we bind a listener that will set a walletChanged flag to callbacks on the Wallet Manager.
            // These callbacks will automatically be invoked anytime a currency balance is changed.
            // This prevents us from having to manually invoke RefreshUI every time we perform one of these actions.
            WalletManager.balanceChanged += OnCoinBalanceChanged;

            // We'll initialize our WalletManager's coin balance with 50 coins.
            // This will set the balance to 50 no matter what it's current balance is.
            WalletManager.GetBalance(m_softCurrencyDefinition);
            WalletManager.GetBalance(m_hardCurrencyDefinition);
            
            RefreshAllCurrencies();
        }

        /// <summary>
        /// Standard Update method for Unity scripts.
        /// </summary>
        private void Update()
        {
            // This flag will be set to true when the balance of a currency has changed in the WalletManager
            if (m_WalletChanged)
            {
                Save();
                RefreshAllCurrencies();
                m_WalletChanged = false;
            }
        }

        /// <summary>
        /// Standard cleanup point for Unity scripts.
        /// </summary>
        private void OnDestroy()
        {
            if (WalletManager.IsInitialized)
                WalletManager.balanceChanged -= OnCoinBalanceChanged;
        }

        /// <summary>
        /// This method adds 50 coins to the wallet.
        /// </summary>
        [ContextMenu("Find")]
        public void FindBagOfCoins()
        {
            WalletManager.AddBalance(m_softCurrencyDefinition, 50);
        }

        /// <summary>
        /// This method deducts 10 coins from the wallet.
        /// </summary>
        [ContextMenu("Drop")]
        public void Drop10Coins()
        {
            WalletManager.RemoveBalance(m_softCurrencyDefinition, 10);
        }

        /// <summary>
        /// This will fill out the main text box with information about the wallet.
        /// </summary>
        private void RefreshCurrency(StringBuilder builder, TMP_Text text, Currency currency)
        {
            builder.Clear();
            
            var coinBalance = WalletManager.GetBalance(currency);
            builder.Append($"{coinBalance.ToString()}");

            text.text = builder.ToString();
        }

        private void RefreshAllCurrencies()
        {
            RefreshCurrency(m_softCurrencyDisplayText, softCurrencyText, m_softCurrencyDefinition);
            RefreshCurrency(m_hardCurrencyDisplayText, hardCurrencyText, m_hardCurrencyDefinition);
        }
        /// <summary>
        /// This will be called every time a currency balance is changed.
        /// </summary>
        /// <param name="args">
        /// Data related to the <see cref="WalletManager.balanceChanged"/> event.
        /// </param>
        private void OnCoinBalanceChanged(BalanceChangedEventArgs args)
        {
            if (args.currency.key != m_softCurrencyDefinition.key)
                return;
            m_WalletChanged = true;
        }
        // Called when Game Foundation is successfully initialized.
        private void OnGameFoundationInitialized()
        {
            WalletManager.balanceChanged += OnCoinBalanceChanged;
        }

        // Called if Game Foundation initialization fails 
        private void OnInitFailed(Exception error)
        {
            Debug.LogException(error);
        }
        
        public void Save()
        {
            // Deferred is a struct that helps you track the progress of an asynchronous operation of Game Foundation.
            Deferred saveOperation = m_DataLayer.Save();

            // Check if the operation is already done.
            if (saveOperation.isDone)
            {
                LogSaveOperationCompletion(saveOperation);
            }
            else
            {
                StartCoroutine(WaitForSaveCompletion(saveOperation));
            }
        }
        [ContextMenu("Load")]
        public void Load()
        {
            // Don't forget to stop listening to events before un-initializing.
            WalletManager.balanceChanged -= OnCoinBalanceChanged;

            GameFoundation.Uninitialize();

            GameFoundation.Initialize(m_DataLayer, OnGameFoundationInitialized, Debug.LogError);
        }
        
        private static IEnumerator WaitForSaveCompletion(Deferred saveOperation)
        {
            // Wait for the operation to complete.
            yield return saveOperation.Wait();

            LogSaveOperationCompletion(saveOperation);
        }

        private static void LogSaveOperationCompletion(Deferred saveOperation)
        {
            // Check if the operation was successful.
            if (saveOperation.isFulfilled)
            {
                Debug.Log("Saved!");
            }
            else
            {
                Debug.LogError($"Save failed! Error: {saveOperation.error}");
            }
        }
    }
}
