using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    private GameManager gameManager;
    private PlayerCombat playerCombat;

    public GameObject shopUI;
    public Weapon[] weaponReferences;
    [SerializeField] Sprite[] upgradePips;

    [Header("Header")]
    [SerializeField] private TMP_Text clamText; //displays player clams
    [SerializeField] private Button closeButton; //closes menu

    [Header("Weapon")]
    [SerializeField] private Button[] purchaseWeaponButtons; //The buttons to click to buy a new weapon
    [SerializeField] private GameObject[] weaponPriceTags; //GameObject with text to display weapon costs
    [Space]
    [SerializeField] private TMP_Text[] swordUpgradeCostText;
    [SerializeField] private TMP_Text[] pistolUpgradeCostText;
    [SerializeField] private TMP_Text[] shotgunUpgradeCostText;
    [SerializeField] private TMP_Text[] rifleUpgradeCostText;
    [SerializeField] private TMP_Text[] SMGUpgradeCostText;
    [SerializeField] private TMP_Text[] minigunUpgradeCostText;
    [Space]
    [SerializeField] private Button[] swordUpgradeButtons;
    [SerializeField] private Button[] pistolUpgradeButtons;
    [SerializeField] private Button[] shotgunUpgradeButtons;
    [SerializeField] private Button[] rifleUpgradeButtons;
    [SerializeField] private Button[] SMGUpgradeButtons;
    [SerializeField] private Button[] minigunUpgradeButtons;
    [Space]
    [SerializeField] private Image[] swordUpgradePips;
    [SerializeField] private Image[] pistolUpgradePips;
    [SerializeField] private Image[] shotgunUpgradePips;
    [SerializeField] private Image[] rifleUpgradePips;
    [SerializeField] private Image[] SMGUpgradePips;
    [SerializeField] private Image[] minigunUpgradePips;

    [Header("Health and Armor")]
    [SerializeField] private Button upgradeHealthButton;
    [SerializeField] private Button upgradeArmorButton;
    [Space]
    public GameObject healthMaxed; //To display that player health is fully upgraded
    public GameObject shieldMaxed; //Same as above but for amor
    [Space]
    [SerializeField] private Image healthUpgradePips;
    [SerializeField] private Image armorUpgradePips;

    private int playerHealthUpgrades;
    private int playerArmorUpgrades;

    private int healthUpgradePrice;
    private int armorUpgradePrice;

    private void Start()
    {
        gameManager = GameManager.instance;
        playerCombat = PlayerController.instance.combat;

        AssignButtonFunctions();
        SetInitialShopDisplay();
        CloseShop();
    }

    private void AssignButtonFunctions()
    {
        closeButton.onClick.AddListener(CloseShop);

        //Sets the buttons to purchase a new weapon
        for (int i = 0; i < purchaseWeaponButtons.Length; i++)
        {
            int index = i;
            purchaseWeaponButtons[index].onClick.AddListener(delegate
            {
                OnPurchaseNewWeapon(index);
            });
        }

        upgradeHealthButton.onClick.AddListener(OnUpgradeHealth);
        upgradeArmorButton.onClick.AddListener(OnUpgradeArmor);

        for (int i = 0; i < swordUpgradeButtons.Length; i++)
        {
            int stat = i;
            swordUpgradeButtons[stat].onClick.AddListener(delegate
            {
                OnUpgradeWeapon((int)WeaponType.Sword, stat);
            });
        }

        for (int i = 0; i < pistolUpgradeButtons.Length; i++)
        {
            int stat = i;
            pistolUpgradeButtons[stat].onClick.AddListener(delegate
            {
                OnUpgradeWeapon((int)WeaponType.Pistol, stat);
            });
        }

        for (int i = 0; i < shotgunUpgradeButtons.Length; i++)
        {
            int stat = i;
            shotgunUpgradeButtons[stat].onClick.AddListener(delegate
            {
                OnUpgradeWeapon((int)WeaponType.Shotgun, stat);
            });
        }

        for (int i = 0; i < rifleUpgradeButtons.Length; i++)
        {
            int stat = i;
            rifleUpgradeButtons[stat].onClick.AddListener(delegate
            {
                OnUpgradeWeapon((int)WeaponType.Rifle, stat);
            });
        }

        for (int i = 0; i < SMGUpgradeButtons.Length; i++)
        {
            int stat = i;
            SMGUpgradeButtons[stat].onClick.AddListener(delegate
            {
                OnUpgradeWeapon((int)WeaponType.SMG, stat);
            });
        }

        for (int i = 0; i < minigunUpgradeButtons.Length; i++)
        {
            int stat = i;
            minigunUpgradeButtons[stat].onClick.AddListener(delegate
            {
                OnUpgradeWeapon((int)WeaponType.MiniGun, stat);
            });
        }
    }

    private void SetInitialShopDisplay()
    {
        //Weapon unlocks
        for (int i = 0; i < playerCombat.PlayerWeapons.Length; i++)
        {
            bool isUnlocked = playerCombat.PlayerWeapons[i].isUnlocked;
            //Price tags are only displayed if the weapon is not unlocked
            weaponPriceTags[i].SetActive(!isUnlocked);
            //The player already owns this weapon, disable the button
            purchaseWeaponButtons[i].interactable = !isUnlocked;
        }

        //Health Upgrades
        playerHealthUpgrades = PlayerController.instance.MaxHealth - 10;
        healthMaxed.SetActive(playerHealthUpgrades >= 5);
        healthUpgradePrice = 100 + (playerHealthUpgrades * 100);
        healthUpgradePips.sprite = upgradePips[playerHealthUpgrades];

        //Armor Upgrades
        playerArmorUpgrades = PlayerController.instance.MaxArmor - 10;
        shieldMaxed.SetActive(playerArmorUpgrades >= 5);
        armorUpgradePrice = 100 + (playerArmorUpgrades * 100);
        armorUpgradePips.sprite = upgradePips[playerArmorUpgrades];

        //Weapon Upgrade Pips
        for (int i = 0; i < System.Enum.GetNames(typeof(WeaponType)).Length; i++)
        {
            UpdateWeaponDisplays(i);
        }
    }

    #region - Weapon Purchase and Upgrades -
    private void OnPurchaseNewWeapon(int weaponIndex)
    {
        //The player has already purchased this weapon
        if (playerCombat.PlayerWeapons[weaponIndex].isUnlocked) return;

        //References the weapon for grabbing prices and stats
        Weapon weaponToBuy = weaponReferences[weaponIndex];

        //Player does not have enough clams
        if (gameManager.playerClams < weaponToBuy.Cost) return;

        //Else, the player has enough money, purchase the weapon
        playerCombat.OnNewWeaponUnlocked(weaponIndex);
        GameManager.OnClamsLost(weaponToBuy.Cost);
        weaponPriceTags[weaponIndex].SetActive(false);
        PlaySoundFX();

        //Update shop display
        clamText.text = gameManager.playerClams.ToString();
        for (int i = 0; i < playerCombat.PlayerWeapons.Length; i++)
        {
            bool isUnlocked = playerCombat.PlayerWeapons[i].isUnlocked;
            //Price tags are only displayed if the weapon is not unlocked
            weaponPriceTags[i].SetActive(!isUnlocked);
            //The player already owns this weapon, disable the button
            purchaseWeaponButtons[i].interactable = !isUnlocked;
        }
    }
    
    private void OnUpgradeWeapon(int weaponIndex, int statIndex)
    {
        switch (statIndex)
        {
            case 0:
                TryUpgradeWeaponDamage(weaponIndex);
                break;
            case 1:
                TryUpgradeWeaponAttacRate(weaponIndex);
                break;
            case 2:
                TryUpgradeWeaponMagazine(weaponIndex);
                break;
            case 3:
                TryUpgradeWeaponAmmoCapacity(weaponIndex);
                break;
        }
    }

    private void TryUpgradeWeaponDamage(int weaponIndex)
    {
        //return if fully upgraded or not enough cash
        int upgradeTier = playerCombat.PlayerWeapons[weaponIndex].damage_Tier;
        if (upgradeTier >= 5) return;

        int clamCost = weaponReferences[weaponIndex].GetDamageUpgradeCost(upgradeTier);
        if (gameManager.playerClams < clamCost) return;

        GameManager.OnClamsLost(clamCost);
        playerCombat.OnWeaponUpgraded(weaponIndex, 0);
        PlaySoundFX();

        UpdateWeaponDisplays(weaponIndex);
    }

    private void TryUpgradeWeaponAttacRate(int weaponIndex)
    {
        //return if fully upgraded  //return if not enough cash
        int upgradeTier = playerCombat.PlayerWeapons[weaponIndex].attackRate_Tier;
        if (upgradeTier >= 5) return;

        int clamCost = weaponReferences[weaponIndex].GetAttackRateUpgradeCost(upgradeTier);
        if (gameManager.playerClams < clamCost) return;

        GameManager.OnClamsLost(clamCost);
        playerCombat.OnWeaponUpgraded(weaponIndex, 1);
        PlaySoundFX();

        UpdateWeaponDisplays(weaponIndex);
    }

    private void TryUpgradeWeaponAmmoCapacity(int weaponIndex)
    {
        //return if fully upgraded  //return if not enough cash
        int upgradeTier = playerCombat.PlayerWeapons[weaponIndex].ammoCapacity_Tier;
        if (upgradeTier >= 5) return;

        int clamCost = weaponReferences[weaponIndex].GetAmmoCapUpgradeCost(upgradeTier);
        if (gameManager.playerClams < clamCost) return;

        GameManager.OnClamsLost(clamCost);
        playerCombat.OnWeaponUpgraded(weaponIndex, 2);
        PlaySoundFX();

        UpdateWeaponDisplays(weaponIndex);
    }

    private void TryUpgradeWeaponMagazine(int weaponIndex)
    {
        //return if fully upgraded  //return if not enough cash
        int upgradeTier = playerCombat.PlayerWeapons[weaponIndex].magazineCapacity_Tier;
        if (upgradeTier >= 5) return;

        int clamCost = weaponReferences[weaponIndex].GetMagazineUpgradeCost(upgradeTier);
        if (gameManager.playerClams < clamCost) return;

        GameManager.OnClamsLost(clamCost);
        playerCombat.OnWeaponUpgraded(weaponIndex, 3);
        PlaySoundFX();

        UpdateWeaponDisplays(weaponIndex);
    }
    #endregion

    #region - Player Upgrades -
    private void OnUpgradeHealth()
    {
        if (playerHealthUpgrades >= 5) return;
        if (gameManager.playerClams < healthUpgradePrice) return;

        //Player can purchase a health upgrade
        PlayerController.instance.OnHealthUpgrade();
        GameManager.OnClamsLost(healthUpgradePrice);
        PlaySoundFX();

        //Update Shop Display
        clamText.text = gameManager.playerClams.ToString();

        playerHealthUpgrades = PlayerController.instance.MaxHealth - 10;
        healthUpgradePrice = 100 + (playerHealthUpgrades * 100);
        healthUpgradePips.sprite = upgradePips[playerHealthUpgrades];

        healthMaxed.SetActive(playerHealthUpgrades >= 5);
        //Or Display Price for next upgrade
    }

    private void OnUpgradeArmor()
    {
        if (playerArmorUpgrades >= 5) return;
        if (gameManager.playerClams < armorUpgradePrice) return;

        //Player can purchase an armor upgrade
        PlayerController.instance.OnArmorUpgrade();
        GameManager.OnClamsLost(armorUpgradePrice);
        PlaySoundFX();

        //Update Shop Display
        clamText.text = gameManager.playerClams.ToString();

        playerArmorUpgrades = PlayerController.instance.MaxArmor - 10;
        armorUpgradePrice = 100 + (playerArmorUpgrades * 100);
        armorUpgradePips.sprite = upgradePips[playerArmorUpgrades];

        shieldMaxed.SetActive(playerArmorUpgrades >= 5);
        //Or Display Price for next upgrade
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OpenShop();
        }
    }

    private void OpenShop()
    {
        shopUI.SetActive(true);
        PlayerController.instance.ToggleMovement(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SetInitialShopDisplay();
    }

    private void CloseShop()
    {
        shopUI.SetActive(false);
        PlayerController.instance.ToggleMovement(true);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void UpdateWeaponDisplays(int weapon)
    {
        clamText.text = gameManager.playerClams.ToString();

        switch (weapon)
        {
            case (int)WeaponType.Sword:
                swordUpgradeCostText[0].text = weaponReferences[0].GetDamageUpgradeCost(playerCombat.PlayerWeapons[0].damage_Tier).ToString();
                swordUpgradeCostText[1].text = weaponReferences[0].GetAttackRateUpgradeCost(playerCombat.PlayerWeapons[0].attackRate_Tier).ToString();
                swordUpgradeCostText[2].text = weaponReferences[0].GetMagazineUpgradeCost(playerCombat.PlayerWeapons[0].magazineCapacity_Tier).ToString();
                swordUpgradeCostText[3].text = weaponReferences[0].GetAmmoCapUpgradeCost(playerCombat.PlayerWeapons[0].ammoCapacity_Tier).ToString();

                swordUpgradePips[0].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.Sword].damage_Tier];
                swordUpgradePips[1].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.Sword].attackRate_Tier];
                swordUpgradePips[2].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.Sword].magazineCapacity_Tier];
                swordUpgradePips[3].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.Sword].ammoCapacity_Tier];
                break;
            case (int)WeaponType.Pistol:
                pistolUpgradeCostText[0].text = weaponReferences[1].GetDamageUpgradeCost(playerCombat.PlayerWeapons[1].damage_Tier).ToString();
                pistolUpgradeCostText[1].text = weaponReferences[1].GetAttackRateUpgradeCost(playerCombat.PlayerWeapons[1].attackRate_Tier).ToString();
                pistolUpgradeCostText[2].text = weaponReferences[1].GetMagazineUpgradeCost(playerCombat.PlayerWeapons[1].magazineCapacity_Tier).ToString();
                pistolUpgradeCostText[3].text = weaponReferences[1].GetAmmoCapUpgradeCost(playerCombat.PlayerWeapons[1].ammoCapacity_Tier).ToString();

                pistolUpgradePips[0].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.Pistol].damage_Tier];
                pistolUpgradePips[1].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.Pistol].attackRate_Tier];
                pistolUpgradePips[2].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.Pistol].magazineCapacity_Tier];
                pistolUpgradePips[3].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.Pistol].ammoCapacity_Tier];
                break;
            case (int)WeaponType.Shotgun:
                shotgunUpgradeCostText[0].text = weaponReferences[2].GetDamageUpgradeCost(playerCombat.PlayerWeapons[2].damage_Tier).ToString();
                shotgunUpgradeCostText[1].text = weaponReferences[2].GetAttackRateUpgradeCost(playerCombat.PlayerWeapons[2].attackRate_Tier).ToString();
                shotgunUpgradeCostText[2].text = weaponReferences[2].GetMagazineUpgradeCost(playerCombat.PlayerWeapons[2].magazineCapacity_Tier).ToString();
                shotgunUpgradeCostText[3].text = weaponReferences[2].GetAmmoCapUpgradeCost(playerCombat.PlayerWeapons[2].ammoCapacity_Tier).ToString();

                shotgunUpgradePips[0].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.Shotgun].damage_Tier];
                shotgunUpgradePips[1].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.Shotgun].attackRate_Tier];
                shotgunUpgradePips[2].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.Shotgun].magazineCapacity_Tier];
                shotgunUpgradePips[3].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.Shotgun].ammoCapacity_Tier];
                break;
            case (int)WeaponType.Rifle:
                rifleUpgradeCostText[0].text = weaponReferences[3].GetDamageUpgradeCost(playerCombat.PlayerWeapons[3].damage_Tier).ToString();
                rifleUpgradeCostText[1].text = weaponReferences[3].GetAttackRateUpgradeCost(playerCombat.PlayerWeapons[3].attackRate_Tier).ToString();
                rifleUpgradeCostText[2].text = weaponReferences[3].GetMagazineUpgradeCost(playerCombat.PlayerWeapons[3].magazineCapacity_Tier).ToString();
                rifleUpgradeCostText[3].text = weaponReferences[3].GetAmmoCapUpgradeCost(playerCombat.PlayerWeapons[3].ammoCapacity_Tier).ToString();

                rifleUpgradePips[0].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.Rifle].damage_Tier];
                rifleUpgradePips[1].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.Rifle].attackRate_Tier];
                rifleUpgradePips[2].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.Rifle].magazineCapacity_Tier];
                rifleUpgradePips[3].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.Rifle].ammoCapacity_Tier];
                break;
            case (int)WeaponType.SMG:
                SMGUpgradeCostText[0].text = weaponReferences[4].GetDamageUpgradeCost(playerCombat.PlayerWeapons[4].damage_Tier).ToString();
                SMGUpgradeCostText[1].text = weaponReferences[4].GetAttackRateUpgradeCost(playerCombat.PlayerWeapons[4].attackRate_Tier).ToString();
                SMGUpgradeCostText[2].text = weaponReferences[4].GetMagazineUpgradeCost(playerCombat.PlayerWeapons[4].magazineCapacity_Tier).ToString();
                SMGUpgradeCostText[3].text = weaponReferences[4].GetAmmoCapUpgradeCost(playerCombat.PlayerWeapons[4].ammoCapacity_Tier).ToString();

                SMGUpgradePips[0].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.SMG].damage_Tier];
                SMGUpgradePips[1].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.SMG].attackRate_Tier];
                SMGUpgradePips[2].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.SMG].magazineCapacity_Tier];
                SMGUpgradePips[3].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.SMG].ammoCapacity_Tier];
                break;
            case (int)WeaponType.MiniGun:
                minigunUpgradeCostText[0].text = weaponReferences[5].GetDamageUpgradeCost(playerCombat.PlayerWeapons[5].damage_Tier).ToString();
                minigunUpgradeCostText[1].text = weaponReferences[5].GetAttackRateUpgradeCost(playerCombat.PlayerWeapons[5].attackRate_Tier).ToString();
                minigunUpgradeCostText[2].text = weaponReferences[5].GetMagazineUpgradeCost(playerCombat.PlayerWeapons[5].magazineCapacity_Tier).ToString();
                minigunUpgradeCostText[3].text = weaponReferences[5].GetAmmoCapUpgradeCost(playerCombat.PlayerWeapons[5].ammoCapacity_Tier).ToString();

                minigunUpgradePips[0].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.MiniGun].damage_Tier];
                minigunUpgradePips[1].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.MiniGun].attackRate_Tier];
                minigunUpgradePips[2].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.MiniGun].magazineCapacity_Tier];
                minigunUpgradePips[3].sprite = upgradePips[playerCombat.PlayerWeapons[(int)WeaponType.MiniGun].ammoCapacity_Tier];
                break;
        }
    }

    private void PlaySoundFX()
    {
        AudioManager.PlayClip("purchase");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (shopUI.activeSelf) CloseShop();
        }
    }
}
