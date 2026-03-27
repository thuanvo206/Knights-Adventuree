using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerTests
{
    private GameObject playerObject;
    private Player player;
    
    private GameObject dmgObj;
    private GameObject healObj;
    private GameObject coinObj;
    private GameObject hudObj;

    [SetUp]
    public void Setup()
    {
        // 1. Tạo các Object phụ trợ
        dmgObj = new GameObject("GiveDamage");
        dmgObj.AddComponent<GiveDamage>().damage = 20;

        healObj = new GameObject("GiveHealth");
        healObj.AddComponent<GiveHealth>().health = 20;

        coinObj = new GameObject("AddCoin");
        coinObj.AddComponent<AddCoin>().coin = 100;
        
        // Mock UI
        hudObj = new GameObject("HUD");
        GameObject coinCanvas = new GameObject("CoinCanvas");
        coinCanvas.transform.SetParent(hudObj.transform);
        GameObject coinTextObj = new GameObject("CoinCounterText");
        coinTextObj.transform.SetParent(coinCanvas.transform);
        coinTextObj.AddComponent<TMPro.TextMeshProUGUI>();

        // 2. Khởi tạo Player
        playerObject = new GameObject("Player");
        playerObject.AddComponent<Rigidbody2D>();
        playerObject.AddComponent<BoxCollider2D>();
        playerObject.AddComponent<CircleCollider2D>();
        playerObject.AddComponent<Animator>();
        playerObject.AddComponent<AudioSource>(); 

        // TẠO OBJECT CON "GroundCheck" ĐỂ FIX LỖI NULL TRONG FIXEDUPDATE
        GameObject groundCheckObj = new GameObject("GroundCheck");
        groundCheckObj.transform.SetParent(playerObject.transform);
        groundCheckObj.transform.localPosition = Vector3.zero;

        player = playerObject.AddComponent<Player>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(playerObject);
        Object.DestroyImmediate(dmgObj);
        Object.DestroyImmediate(healObj);
        Object.DestroyImmediate(coinObj);
        Object.DestroyImmediate(hudObj);
    }

    [UnityTest]
    public IEnumerator Test_InitialCoin_IsZero()
    {
        yield return null; 
        Assert.AreEqual(0, player.currentCoin);
    }

    [UnityTest]
    public IEnumerator Test_AddCoin_Once()
    {
        yield return null;
        player.earnCoin = true; 
        yield return null; 
        Assert.AreEqual(100, player.currentCoin);
    }

    [UnityTest]
    public IEnumerator Test_InitialHealth_IsMax()
    {
        yield return null;
        Assert.AreEqual(100, player.currentPlayerHealth);
    }

    [UnityTest]
    public IEnumerator Test_TakeDamage_ReducesHealth()
    {
        yield return null;
        player.isHurt = true;
        yield return null; 
        Assert.AreEqual(80, player.currentPlayerHealth);
    }

    [UnityTest]
    public IEnumerator Test_Health_DoesNotExceedMax()
    {
        yield return null;
        player.currentPlayerHealth = 90;
        player.addHealth = true; 
        yield return null; 
        Assert.AreEqual(100, player.currentPlayerHealth);
    }

    [UnityTest]
    public IEnumerator Test_CoinItem_IsDestroyed_OnPickup()
    {
        GameObject testCoinObj = new GameObject("TestCoin");
        testCoinObj.tag = "PlayerItem"; 
        AddCoin addCoinScript = testCoinObj.AddComponent<AddCoin>();
        
        yield return null;

        GameObject dummyPlayer = new GameObject("Dummy");
        dummyPlayer.tag = "Player";
        BoxCollider2D collider = dummyPlayer.AddComponent<BoxCollider2D>();
        
        addCoinScript.SendMessage("OnTriggerEnter2D", collider);
        
        yield return null; 

        Assert.IsTrue(testCoinObj == null);
        Object.DestroyImmediate(dummyPlayer);
    }
    // ==========================================
    // CÁC TEST CASE MỞ RỘNG (Dựa trên logic game thực tế)
    // ==========================================

    [UnityTest]
    public IEnumerator Test_Player_Death_By_Zero_HP()
    {
        yield return null;
        
        // Cố tình set máu về 0
        player.currentPlayerHealth = 0;
        yield return null; // Đợi Update() chạy để kiểm tra điều kiện isDead
        
        // Trong Player.cs có dòng: isDead = currentPlayerHealth <= 0;
        Assert.IsTrue(player.isDead, "Player đáng lẽ phải ở trạng thái isDead = true khi máu = 0");
    }

    [UnityTest]
    public IEnumerator Test_Player_Death_By_Falling_Out_Of_Bounds()
    {
        yield return null;
        
        // Đưa player rơi xuống dưới mốc y = -6
        player.transform.position = new Vector3(0, -7f, 0);
        yield return null; // Đợi Update() chạy
        
        // Trong Player.cs có logic: if (transform.position.y <= -6) isDead = true;
        Assert.IsTrue(player.isDead, "Player đáng lẽ phải chết khi rơi xuống vực (y <= -6)");
    }

    [UnityTest]
    public IEnumerator Test_Enemy_Takes_Damage_From_Player()
    {
        yield return null;

        // 1. Tạo một Kẻ thù giả lập dựa theo class EnemyHealth.cs
        GameObject enemyObj = new GameObject("Enemy");
        enemyObj.AddComponent<SpriteRenderer>();
        enemyObj.AddComponent<CircleCollider2D>();
        enemyObj.AddComponent<Rigidbody2D>();
        
        EnemyHealth enemyHealth = enemyObj.AddComponent<EnemyHealth>();
        enemyHealth.maxEnemyHealth = 100;
        enemyHealth.playerDamageToEnemy = 30; // Giả sử Player đánh mất 30 máu
        
        // Gắn deathParticle để tránh lỗi NullReferenceException trong EnemyHealth
        enemyHealth.deathParticle = new GameObject("DeathParticle"); 
        enemyHealth.deathParticle.SetActive(false);

        yield return null; // Đợi Start() của EnemyHealth chạy để set máu hiện tại = 100

        // 2. Bật cờ cho phép Player gây sát thương
        player.canDamage = true; 

        // 3. Giả lập vũ khí của player (phải có tag "PlayerItem" theo logic EnemyHealth)
        GameObject playerWeapon = new GameObject("Weapon");
        playerWeapon.tag = "PlayerItem";
        BoxCollider2D weaponCollider = playerWeapon.AddComponent<BoxCollider2D>();

        // 4. Cho vũ khí chạm vào Enemy
        enemyHealth.SendMessage("OnTriggerEnter2D", weaponCollider);

        yield return null;

        // 5. Kiểm tra kết quả: 100 máu ban đầu - 30 sát thương = 70 máu
        Assert.AreEqual(70, enemyHealth.currentEnemyHealth);

        // Dọn dẹp rác
        Object.DestroyImmediate(enemyObj);
        Object.DestroyImmediate(enemyHealth.deathParticle);
        Object.DestroyImmediate(playerWeapon);
    }
}