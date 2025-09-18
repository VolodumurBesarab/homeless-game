using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttributes : MonoBehaviour
{
    public static PlayerAttributes Instance;

    [SerializeField] public int health = 20;
    [SerializeField] public int attack = 0;
    [SerializeField] public int haste = 0;
    [SerializeField] public int defence = 0;
    [SerializeField] public int money = 0;
    [SerializeField] public TextMeshProUGUI displayedHealth;
    [SerializeField] public TextMeshProUGUI displayedAttack;
    [SerializeField] public TextMeshProUGUI displayedHaste;
    [SerializeField] public TextMeshProUGUI displayedDefence;
    [SerializeField] public TextMeshProUGUI displayedMoney;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateStatsInUA();
    }

    public void UpdateStatsInUI()
    {
        displayedHealth.text = health.ToString();
        displayedAttack.text = attack.ToString();
        displayedHaste.text = haste.ToString();
        displayedDefence.text = defence.ToString();
        displayedMoney.text = money.ToString();
    }
}
