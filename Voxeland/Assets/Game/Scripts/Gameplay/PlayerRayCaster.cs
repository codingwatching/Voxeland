using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRayCaster : MonoBehaviour
{
    [SerializeField] GameObject cube;
    [SerializeField] VoxelMaster master {get => GameManager.Instance.m_VoxelMaster; }
    [SerializeField] Image radial;
    [SerializeField] Image selected;
    [SerializeField] Sprite[] icons;
    Vector3Int? _lastVoxelPosInt = new Vector3Int();
    int currentID = 0;
    int lastID = 1;

    void Start() { cube = GameObject.Instantiate(cube, Vector3.zero, Quaternion.identity); }

    void Update()
    {
        DoVoxelSelectedID();

        RaycastHit hit = GameManager.Instance.HitRayCast(20);
        bool b = GameManager.Instance.BoolRayCast(20);
        cube.SetActive(b);
        radial.gameObject.SetActive(b);
        if (!b) return;

        DoVoxelManipulation(
            hit.point - hit.normal * 0.5f,
            hit.point + hit.normal * 0.5f);

    }

    void DoVoxelSelectedID()
    {
        currentID += Mathf.FloorToInt(Input.mouseScrollDelta.y);
        currentID = currentID < 0 ? icons.Length - 1 : currentID >= icons.Length ? 0 : currentID;
        if (currentID != lastID)
            selected.sprite = icons[currentID];
        lastID = currentID;
    }

    void DoVoxelManipulation(Vector3 _voxelPos, Vector3 _airPos)
    {
        Vector3Int VoxelPosInt = Vector3Int.FloorToInt(_voxelPos);
        cube.transform.position = VoxelPosInt;

        //Break Voxel
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0) || _lastVoxelPosInt != VoxelPosInt)
        {
            StopAllCoroutines();
            _lastVoxelPosInt = null;
            radial.gameObject.SetActive(false);
        }
        if (Input.GetMouseButton(0))
            if (_lastVoxelPosInt != VoxelPosInt)
                StartCoroutine(RemoveVoxel(_voxelPos));

        //Place Voxel
        if (Input.GetMouseButtonDown(1))
            if (NotInPlayer(_airPos))
            {
                master.SetVoxelID(_airPos, (short)currentID);
                master.FastRefresh();
            }
    }


    bool NotInPlayer(Vector3 _p, bool _crouching = false)
    {
        Vector3 camPos = Vector3Int.FloorToInt(GameManager.Instance.m_MainCamera.gameObject.transform.position);
        Vector3 airBlock = Vector3Int.FloorToInt(_p);

        if (airBlock == camPos) return false;
        if (!_crouching)
            if (airBlock == Vector3.down + camPos)
                return false;

        return true;
    }
    IEnumerator RemoveVoxel(Vector3 _pos)
    {
        Voxel voxel = master.GetVoxel(_pos);

        if (voxel != null)
        {
            float startTime = Time.time;
            float endTime = startTime + voxel.Info.Durability;
            while (Time.time < endTime)
            {
                _lastVoxelPosInt = Vector3Int.FloorToInt(_pos);
                radial.gameObject.SetActive(true);
                radial.fillAmount = (Time.time - startTime) / voxel.Info.Durability;
                yield return new WaitForEndOfFrame();
            }

            master.RemoveVoxelAt(_pos);
            master.FastRefresh();
        }

        yield return null;
    }
}
