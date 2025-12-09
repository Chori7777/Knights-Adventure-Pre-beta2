using UnityEngine;

[System.Serializable]
public class NPCReward
{
    public enum RewardType { Potion, Axe, MaxHealth, MaxAxes, MaxPotions, AttackDamage }

    public RewardType type;
    public int amount = 1;

    public void Apply()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        playerLife vida = playerObj.GetComponent<playerLife>();
        if (vida == null) return;

        var datos = ControladorDatosJuego.Instance?.datosjuego;
        if (datos == null) return;

        switch (type)
        {
            case RewardType.Potion:
                vida.AddPotion(amount);
                Debug.Log($"[NPCReward] +{amount} poción");
                break;

            case RewardType.Axe:
                datos.cantidadHachas = Mathf.Min(datos.cantidadHachas + amount, datos.maxHachas);
                ControladorDatosJuego.Instance.GuardarDatos(false);
                if (PlayerHealthUI.Instance != null)
                    PlayerHealthUI.Instance.ActualizarHachas(datos.cantidadHachas);
                Debug.Log($"[NPCReward] +{amount} hacha");
                break;

            case RewardType.MaxHealth:
                int newMax = vida.MaxHealth + amount;
                vida.SetMaxHealth(newMax);
                vida.SetHealth(newMax);
                datos.vidaMaxima = newMax;
                ControladorDatosJuego.Instance.GuardarDatos(false);
                Debug.Log($"[NPCReward] +{amount} vida máxima (ahora {newMax})");
                break;

            case RewardType.MaxAxes:
                datos.maxHachas += amount;
                ControladorDatosJuego.Instance.GuardarDatos(false);
                if (PlayerHealthUI.Instance != null)
                    PlayerHealthUI.Instance.ActualizarHachas(datos.cantidadHachas);
                Debug.Log($"[NPCReward] Máximo de hachas aumentado a {datos.maxHachas}");
                break;

            case RewardType.MaxPotions:
                vida.SetMaxPotions(vida.MaxPotions + amount);
                datos.maxPotions = vida.MaxPotions;
                ControladorDatosJuego.Instance.GuardarDatos(false);
                Debug.Log($"[NPCReward] Máximo de pociones aumentado a {vida.MaxPotions}");
                break;

            case RewardType.AttackDamage:
                datos.attackDamageUpgrades += amount;
                ControladorDatosJuego.Instance.GuardarDatos(false);
                Debug.Log($"[NPCReward] +{amount} daño de ataque (total upgrades: {datos.attackDamageUpgrades})");
                break;
        }
    }
}
