using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;

public class PlayerManagerScript : NetworkComponent
{
    string[] args;
    public List<Vector3> locations;
    public override void HandleMessage(string flag, string value)
    {
        if(flag == "LOGIN" && IsServer)
        {
            args = value.Split(',');
            StartCoroutine(SetStats());
        }
    }

    public override void NetworkedStart()
    {
        if (IsLocalPlayer)
        {
            var temp = FindObjectOfType<OfflinePlayerHolder>();
            SendCommand("LOGIN",
                OfflinePlayerHolder.PlayerPrefab + "," + OfflinePlayerHolder.PreviousScene + "," + temp.PName + "," +
                temp.Health + "," +
                temp.Stamina + "," +
                temp.EXP + "," +
                temp.EXPToLevel + "," +
                temp.Level + "," +
                temp.MoveSpeedMod + "," +
                temp.HealthMod + "," +
                temp.DamageMod + "," +
                temp.HealthRegenerationMod + "," +
                temp.AttackSpeedMod + "," +
                temp.EXPMod + "," +
                temp.StaminaMod + "," +
                temp.MoveSpeedUpgrade + "," +
                temp.HealthUpgrade + "," +
                temp.DamageUpgrade + "," +
                temp.HealthRegenerationUpgrade + "," +
                temp.AttackSpeedUpgrade + "," +
                temp.EXPModUpgrade + "," +
                temp.StaminaUpgrade + "," +
                temp.item1ID + "," + temp.item2ID + "," + temp.item3ID + "," + temp.item4ID + "," + temp.item5ID);
        }
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(0.1f);
    }

    public IEnumerator SetStats()
    {
        yield return new WaitUntil(() => IsConnected);
        int type = int.Parse(args[0]);
        int lastScene = int.Parse(args[1]);
        string pn = args[2];
        float Health = float.Parse(args[3]);
        float Stamina = float.Parse(args[4]);
        int EXP = int.Parse(args[5]);
        int EXPToLevel = int.Parse(args[6]);
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

        int item1ID = int.Parse(args[22]);
        int item2ID = int.Parse(args[23]);
        int item3ID = int.Parse(args[24]);
        int item4ID = int.Parse(args[25]);
        int item5ID = int.Parse(args[26]);

        GameObject spawnLocation = GameObject.Find(lastScene.ToString());
        if (spawnLocation == null && lastScene != 0)
        {
            throw new System.Exception("Could not find spawn location");
        }
        GameObject temp;
        if (lastScene != 0)
        {
            temp = MyCore.NetCreateObject(type, Owner, spawnLocation.transform.position, Quaternion.identity);
            PlayerController tempPC = temp.GetComponent<PlayerController>();

            tempPC.StartCoroutine(tempPC.SetName(pn));
            tempPC.Health = Health;
            tempPC.Stamina = Stamina;

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

            tempPC.StartCoroutine(tempPC.LevelTimer(Level, EXP));
            tempPC.StartCoroutine(tempPC.ReplaceItems(item1ID, item2ID, item3ID, item4ID, item5ID));
            tempPC.teleport = true;
        }
        else
        {
            locations.Clear();
            foreach (GameObject l in GameObject.FindGameObjectsWithTag("PLAYERSPAWN"))
            {
                locations.Add(l.transform.position);
            }

            int rand = Random.Range(0, locations.Count);

            temp = MyCore.NetCreateObject(type, Owner, locations[rand], Quaternion.identity);
            temp.GetComponent<PlayerController>().StartCoroutine(temp.GetComponent<PlayerController>().SetName(pn));
        }
    }
}
