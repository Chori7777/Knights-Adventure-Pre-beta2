using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DatosJuego
{

    public int vidaActual = 5;
    public int vidaMaxima = 5;

  
    public int cantidadpociones = 3;
    public int maxPotions = 5;


    public int cantidadHachas = 3;      
    public int maxHachas = 3;          


    public int nivelActualEspada = 1;


    public int cantidadMonedas = 0;


    public int level = 1;


    public Vector3 posicion = Vector3.zero;
    public Vector3 posicionCamara = Vector3.zero;


    public string escenaActual = "";

  
    public List<string> jefesDerrotados = new List<string>();

    // HABILIDADES DESBLOQUEADAS 
    public bool hasShield = false;
    public bool hasWallCling = true;      
    public bool hasDoubleJump = true;    
    public bool hasDash = true;           
    public bool hasRangedAttack = false;

    //  MEJORAS DE STATS 
    public int maxHealthUpgrades = 0;    
    public int attackDamageUpgrades = 0; 
    public int maxAxesUpgrades = 0;       
    public int maxPotionsUpgrades = 0;    
}