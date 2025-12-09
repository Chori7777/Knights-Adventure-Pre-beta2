using UnityEngine;

public class swordDamageScript : MonoBehaviour
{
    private int GetSwordDamage()
    {
        if (ControladorDatosJuego.Instance == null)
            return 1;  // Da�o por defecto si no hay datos

        DatosJuego datos = ControladorDatosJuego.Instance.datosjuego;

        // EJEMPLO: Da�o base + mejoras de ataque + nivel del arma
        int damage = 1
                     + datos.attackDamageUpgrades
                     + (datos.nivelActualEspada - 1);

        return Mathf.Max(damage, 1);  // Evita da�o 0
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        int damage = GetSwordDamage();

        if (other.CompareTag("enemy"))
        {
            Debug.Log($"Hit enemy! Damage: {damage}");

            var life = other.GetComponent<EnemyLife>();
            if (life != null)
                life.TakeDamageWithKnockback(transform.position, damage);
        }

        if (other.CompareTag("Boss"))
        {
            Debug.Log($"Hit BOSS! Damage: {damage}");

            var life = other.GetComponent<BossLife>();
            if (life != null)
                life.RecibeDanio(transform.position, damage);
        }
    }
}
