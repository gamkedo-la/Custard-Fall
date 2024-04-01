using UnityEngine;

public class Monolyth : MonoBehaviour
{
    private UpgradeableStructure _upgrader;
    private Tidesmanager _tidesmanager;

    [SerializeField] private VfxInstance vfxOnActivated;
    [SerializeField] private Transform vfxSpawn;
    
    void Awake()
    {
        _upgrader = GetComponent<UpgradeableStructure>();
        _upgrader.OnLevelUp += ShrinkCustard;

        _tidesmanager = FindObjectOfType<Tidesmanager>();
    }

    [ContextMenu("VFX")]
    public void PlayVFX()
    {
        if (vfxOnActivated)
            VfxInstance.Spawn(vfxOnActivated, vfxSpawn.position,Quaternion.identity);
    }

    private void ShrinkCustard(object sender, UpgradeableStructure.UpgradeArgs e)
    {
        PlayVFX();
        var tidesmanagerIndexOfCurrentDayTimeTideLevel = _tidesmanager.indexOfCurrentDayTimeTideLevel - 1;
        if (tidesmanagerIndexOfCurrentDayTimeTideLevel < 0)
        {
            tidesmanagerIndexOfCurrentDayTimeTideLevel = _tidesmanager.CurrentMaxDayTimeIndex();
        }
        if (e.maxedOut)
        {
            Debug.Log("set custard level to 1 layer");
            _tidesmanager.OverrideTideStep(new Tidesmanager.TideStep(1),tidesmanagerIndexOfCurrentDayTimeTideLevel, Reset);
        }
        else
        {
            Debug.Log("decrease custard level to 3");
            _tidesmanager.OverrideTideStep(new Tidesmanager.TideStep(3),tidesmanagerIndexOfCurrentDayTimeTideLevel);
        }
    }

    public void Reset()
    {
        _upgrader.ForceSetCurrentLevel(1);
    }
}
