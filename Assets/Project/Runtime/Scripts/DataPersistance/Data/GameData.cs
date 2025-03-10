using UnityEngine;

[System.Serializable]
public class GameData {
    public int deathCount;
    public int killRecord;

    public GameData() {
        this.deathCount = 0;
        this.killRecord = 0;
    }
}
