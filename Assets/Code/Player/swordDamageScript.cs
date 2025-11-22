using UnityEngine;

public class swordDamageScript : MonoBehaviour
{
    private int GetSwordDamage()
    {
        if (ControladorDatosJuego.Instance == null)
            return 1;  // Daño por defecto si no hay datos

        DatosJuego datos = ControladorDatosJuego.Instance.datosjuego;

        // EJEMPLO: Daño base + mejoras de ataque + nivel del arma
        int damage = 1
                     + datos.attackDamageUpgrades
                     + (datos.nivelActualEspada - 1);

        return Mathf.Max(damage, 1);  // Evita daño 0
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        int damage = GetSwordDamage();

        if (other.CompareTag("enemy"))
        {
            Debug.Log($"Hit enemy! Damage: {damage}");

            var life = other.GetComponent<EnemyLife>();
            if (life != null)
                life.TakeDamage(damage);
        }

        if (other.CompareTag("Boss"))
        {
            Debug.Log($"Hit BOSS! Damage: {damage}");

            var life = other.GetComponent<BossLife>();
            if (life != null)
                life.TakeDamage(damage);
        }
    }
}
