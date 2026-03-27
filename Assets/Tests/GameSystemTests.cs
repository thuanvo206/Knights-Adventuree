using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI; 

public class GameSystemTests
{
    // Biến lưu trữ các object cần dọn dẹp
    private GameObject playerObj;
    private GameObject mockEnvObj;

    [SetUp]
    public void Setup()
    {
        // 1. TẠO MÔI TRƯỜNG GIẢ LẬP ĐỂ PLAYER KHÔNG BỊ LỖI NULL
        mockEnvObj = new GameObject("MockEnvironment");

        new GameObject("GiveDamage").transform.SetParent(mockEnvObj.transform);
        mockEnvObj.transform.GetChild(0).gameObject.AddComponent<GiveDamage>();

        new GameObject("GiveHealth").transform.SetParent(mockEnvObj.transform);
        mockEnvObj.transform.GetChild(1).gameObject.AddComponent<GiveHealth>();

        new GameObject("AddCoin").transform.SetParent(mockEnvObj.transform);
        mockEnvObj.transform.GetChild(2).gameObject.AddComponent<AddCoin>();

        GameObject hud = new GameObject("HUD");
        hud.transform.SetParent(mockEnvObj.transform);
        GameObject coinCanvas = new GameObject("CoinCanvas");
        coinCanvas.transform.SetParent(hud.transform);
        GameObject textObj = new GameObject("CoinCounterText");
        textObj.transform.SetParent(coinCanvas.transform);
        textObj.AddComponent<TMPro.TextMeshProUGUI>();

        // 2. TẠO PLAYER VỚI ĐẦY ĐỦ LINH KIỆN (Để UpdateAnimations không bị sập)
        playerObj = new GameObject("Player");
        playerObj.AddComponent<Rigidbody2D>();
        playerObj.AddComponent<BoxCollider2D>();
        playerObj.AddComponent<CircleCollider2D>();
        playerObj.AddComponent<Animator>();
        playerObj.AddComponent<AudioSource>();

        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(playerObj.transform);

        // Cuối cùng mới gắn script Player (để Start() chạy mượt mà)
        playerObj.AddComponent<Player>();
    }

    [TearDown]
    public void Teardown()
    {
        // Dọn dẹp sạch sẽ sau mỗi bài test
        Time.timeScale = 1.0f; // Trả lại thời gian bình thường nếu test trước lỡ Pause
        Object.DestroyImmediate(playerObj);
        Object.DestroyImmediate(mockEnvObj);
    }

    // ==========================================
    // 1. TEST GAMEMANAGER: Tạm dừng game
    // ==========================================
    [UnityTest]
    public IEnumerator Test_GameManager_PauseGame_TogglesTimeScale()
    {
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();
        
        GameObject sliderObj = new GameObject("Slider");
        gm.healthBar = sliderObj.AddComponent<Slider>();

        GameObject pauseUI = new GameObject("PauseUI");
        pauseUI.SetActive(false);
        gm.pauseGame = pauseUI;

        yield return null; // Đợi các hàm Start() chạy xong

        // Tạm dừng
        gm.PauseGame();
        Assert.AreEqual(0.0f, Time.timeScale, "TimeScale phải bằng 0 khi Pause");
        Assert.IsTrue(pauseUI.activeSelf, "UI Pause phải được bật");

        // Tiếp tục
        gm.PauseGame();
        Assert.AreEqual(1.0f, Time.timeScale, "TimeScale phải bằng 1 khi Resume");
        Assert.IsFalse(pauseUI.activeSelf, "UI Pause phải được tắt");

        Object.DestroyImmediate(gmObj);
        Object.DestroyImmediate(pauseUI);
        Object.DestroyImmediate(sliderObj);
    }

    // ==========================================
    // 2. TEST GAMEMANAGER: Cập nhật thanh máu UI
    // ==========================================
    [UnityTest]
    public IEnumerator Test_GameManager_UI_SyncsWithPlayerHealth()
    {
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();
        
        GameObject sliderObj = new GameObject("Slider");
        Slider healthSlider = sliderObj.AddComponent<Slider>();
        gm.healthBar = healthSlider;

        yield return null; 

        // Ép máu player tụt xuống còn 45
        Player player = playerObj.GetComponent<Player>();
        player.currentPlayerHealth = 45;
        
        yield return null; // Đợi Update() của GameManager chạy

        Assert.AreEqual(45, healthSlider.value);

        Object.DestroyImmediate(gmObj);
        Object.DestroyImmediate(sliderObj);
    }

    // ==========================================
    // 3. TEST ENEMY: Logic chết của quái vật
    // ==========================================
    [UnityTest]
    public IEnumerator Test_EnemyHealth_DeathSequence()
    {
        GameObject enemyObj = new GameObject("Enemy");
        SpriteRenderer sr = enemyObj.AddComponent<SpriteRenderer>();
        CircleCollider2D col = enemyObj.AddComponent<CircleCollider2D>();
        enemyObj.AddComponent<Rigidbody2D>();
        
        EnemyHealth enemyHealth = enemyObj.AddComponent<EnemyHealth>();
        enemyHealth.maxEnemyHealth = 100;
        
        GameObject deathVFX = new GameObject("DeathVFX");
        deathVFX.SetActive(false);
        enemyHealth.deathParticle = deathVFX;

        yield return null;

        enemyHealth.currentEnemyHealth = 0;
        yield return null; 

        Assert.IsFalse(sr.enabled, "Hình ảnh quái vật chưa tắt");
        Assert.IsFalse(col.enabled, "Va chạm quái vật chưa tắt");
        Assert.IsTrue(deathVFX.activeSelf, "Hiệu ứng nổ chưa bật");

        Object.DestroyImmediate(deathVFX);
        Object.DestroyImmediate(enemyObj);
    }

    // ==========================================
    // 4. TEST OPENING MANAGER: Bật tắt UI Menu
    // ==========================================
    [UnityTest]
    public IEnumerator Test_OpeningManager_TogglesInfoUI()
    {
        GameObject managerObj = new GameObject("OpeningManager");
        OpeningManager manager = managerObj.AddComponent<OpeningManager>();
        managerObj.AddComponent<AudioSource>(); 
        
        GameObject infoUI = new GameObject("InfoUI");
        infoUI.SetActive(false); 
        manager.infoGame = infoUI;

        yield return null;

        manager.InfoGame();
        
        Assert.IsTrue(infoUI.activeSelf);

        Object.DestroyImmediate(managerObj);
        Object.DestroyImmediate(infoUI);
    }
}