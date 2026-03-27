using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ActionTests
{
    // ==========================================
    // 1. TEST CHUYỂN ĐỘNG CỦA QUÁI VẬT (Dùng Slime làm đại diện)
    // ==========================================
    [UnityTest]
    public IEnumerator Test_Enemy_Movement_Direction()
    {
        // Khởi tạo quái vật Slime
        GameObject enemyObj = new GameObject("Slime");
        Rigidbody2D rb = enemyObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Tắt trọng lực để nó không rơi tự do
        
        SlimeMonsterControl slime = enemyObj.AddComponent<SlimeMonsterControl>();
        slime.enemySpeed = 5f;

        // Cấu hình layer mặt đất là 1 (tương ứng với layer Default trong Unity)
        slime.groundLayer = 1; 

        // Đặt GroundCheck tít lên trời để nó không đụng tường -> isGround = false
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(enemyObj.transform);
        groundCheck.transform.localPosition = new Vector3(0, 100f, 0);

        // Đặt EdgeCheck ngay tâm để chạm đất
        GameObject edgeCheck = new GameObject("EdgeCheck");
        edgeCheck.transform.SetParent(enemyObj.transform);
        edgeCheck.transform.localPosition = Vector3.zero;

        // Lót "thảm" giả để EdgeCheck chạm vào -> onEdge = true (Không bị quay đầu)
        GameObject mockGround = new GameObject("MockGround");
        mockGround.layer = 0; // Layer Default
        BoxCollider2D col = mockGround.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        mockGround.transform.position = Vector3.zero;

        yield return null; // Đợi Start() chạy

        // Trường hợp 1: Quái vật đi sang phải
        slime.moveRight = true;
        yield return null; // Đợi Update() chạy
        Assert.IsTrue(rb.linearVelocity.x > 0, "Vận tốc trục X phải là số dương khi đi sang phải");

        // Trường hợp 2: Quái vật đi sang trái
        slime.moveRight = false;
        yield return null; // Đợi Update() chạy
        Assert.IsTrue(rb.linearVelocity.x < 0, "Vận tốc trục X phải là số âm khi đi sang trái");

        // Dọn dẹp
        Object.DestroyImmediate(enemyObj);
        Object.DestroyImmediate(mockGround);
    }

    // ==========================================
    // 2. TEST PLAYER NHẢY (Jump)
    // ==========================================
    [UnityTest]
    public IEnumerator Test_Player_Jump_AppliesUpwardForce()
    {
        // 1. Dựng môi trường cho Player
        GameObject mockEnvObj = new GameObject("MockEnvironment");
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

        // 2. Tạo Player
        GameObject playerObj = new GameObject("Player");
        Rigidbody2D rb = playerObj.AddComponent<Rigidbody2D>();
        playerObj.AddComponent<BoxCollider2D>();
        playerObj.AddComponent<CircleCollider2D>();
        playerObj.AddComponent<Animator>();
        playerObj.AddComponent<AudioSource>();

        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(playerObj.transform);

        Player player = playerObj.AddComponent<Player>();
        player.jumpPower = 1000f; // Lực nhảy

        yield return null;

        // Hành động: Gọi hàm Jump()
        player.Jump();
        
        yield return new WaitForFixedUpdate(); // Đợi Unity áp dụng lực (AddForce) trong vật lý

        // Kiểm tra: Vận tốc trục Y phải lớn hơn 0 (đang bay lên)
        Assert.IsTrue(rb.linearVelocity.y > 0, "Player phải có vận tốc hướng lên sau khi nhảy");

        Object.DestroyImmediate(playerObj);
        Object.DestroyImmediate(mockEnvObj);
    }
}