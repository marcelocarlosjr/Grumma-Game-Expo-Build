using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class PlayerManagerScript : NetworkComponent
{
    public override void HandleMessage(string flag, string value)
    {
        if(flag == "LOGIN" && IsServer)
        {
            string[] args = value.Split(',');
            int type = int.Parse(args[0]);
            int lastScene = int.Parse(args[1]);
            string pn = args[2];
            float Health = float.Parse(args[3]);
            float Stamina = float.Parse(args[4]);
            float EXP = float.Parse(args[5]);
            float EXPToLevel = float.Parse(args[6]);
            int Level = int.Parse(args[7]);
            float MoveSpeedMod = float.Parse(args[8]);
            float HealthMod = float.Parse(args[9]);
            float DamageMod = float.Parse(args[10]);
            float HealthRegenerationMod = float.Parse(args[11]);
            float AttackSpeedMod = float.Parse(args[12]);
            float EXPMod = float.Parse(args[13]);
            float StaminaMod = float.Parse(args[14]);
            float MoveSpeedUpgrade = float.Parse(args[15]);
            float HealthUpgrade = float.Parse(args[16]);
            float DamageUpgrade = float.Parse(args[17]);
            float HealthRegenerationUpgrade = float.Parse(args[18]);
            float AttackSpeedUpgrade = float.Parse(args[19]);
            float EXPModUpgrade = float.Parse(args[20]);
            float StaminaUpgrade = float.Parse(args[21]);


            GameObject spawnLocation = GameObject.Find(lastScene.ToString());
            if(spawnLocation == null && lastScene != 0)
            {
                throw new System.Exception("Could not find spawn location");
            }
            GameObject temp;
            if(lastScene != 0)
            {
                temp = MyCore.NetCreateObject(type, Owner, spawnLocation.transform.position, Quaternion.identity);
            }
            else
            {
                temp = MyCore.NetCreateObject(type, Owner, Vector3.zero, Quaternion.identity);

            }
            var tempPC = temp.GetComponent<PlayerController>();
            tempPC.Name = pn;
            tempPC.Health = Health;
            tempPC.Stamina = Stamina;
            tempPC.EXP = EXP;
            tempPC.EXPToLevel = EXPToLevel;
            tempPC.Level = Level;

            tempPC.MoveSpeedMod = MoveSpeedMod;
            tempPC.HealthMod = HealthMod;
            tempPC.DamageMod = DamageMod;
            tempPC.HealthRegenerationMod = HealthRegenerationMod;
            tempPC.AttackSpeedMod = AttackSpeedMod;
            tempPC.EXPMod = EXPMod;
            tempPC.StaminaMod = StaminaMod;

            tempPC.MoveSpeedUpgrade = MoveSpeedUpgrade;
            tempPC.HealthUpgrade = HealthUpgrade;
            tempPC.DamageUpgrade = DamageUpgrade;
            tempPC.HealthRegenerationUpgrade = HealthRegenerationUpgrade;
            tempPC.AttackSpeedUpgrade = AttackSpeedUpgrade;
            tempPC.EXPModUpgrade = EXPModUpgrade;
            tempPC.StaminaUpgrade = StaminaUpgrade;
        }
    }

    public override void NetworkedStart()
    {
        if (IsLocalPlayer)
        {

            SendCommand("LOGIN",
                OfflinePlayerHolder.PlayerPrefab + "," + OfflinePlayerHolder.PreviousScene + "," + OfflinePlayerHolder.PName + "," +
                OfflinePlayerHolder.Health + "," +
                OfflinePlayerHolder.Stamina + "," +
                OfflinePlayerHolder.EXP + "," +
                OfflinePlayerHolder.EXPToLevel + "," +
                OfflinePlayerHolder.Level + "," +
                OfflinePlayerHolder.MoveSpeedMod + "," +
                OfflinePlayerHolder.HealthMod + "," +
                OfflinePlayerHolder.DamageMod + "," +
                OfflinePlayerHolder.HealthRegenerationMod + "," +
                OfflinePlayerHolder.AttackSpeedMod + "," +
                OfflinePlayerHolder.EXPMod + "," +
                OfflinePlayerHolder.StaminaMod + "," +
                OfflinePlayerHolder.MoveSpeedUpgrade + "," +
                OfflinePlayerHolder.HealthUpgrade + "," +
                OfflinePlayerHolder.DamageUpgrade + "," +
                OfflinePlayerHolder.HealthRegenerationUpgrade + "," +
                OfflinePlayerHolder.AttackSpeedUpgrade + "," +
                OfflinePlayerHolder.EXPModUpgrade + "," +
                OfflinePlayerHolder.StaminaUpgrade);
        }
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(0.1f);
    }
}
