using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tree_gen : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] tree_prefabs;
    public List<GameObject> instance;
    public int tree_amount = 30;
    public float nearest_dis = 10;
    public float gen_scope = 15; //inside circle radius
    public float exist_scope = 30; //outside circle radius

    public Vector3 random_offset() {
        Vector2 offset = Random.insideUnitCircle * gen_scope;
        while (offset.magnitude < nearest_dis) {
            offset = Random.insideUnitCircle * gen_scope;
        }
        return new Vector3(offset.x,0,offset.y);
    }

    public bool inside_exist_scope(Vector3 pos) {
        return Vector3.Distance(pos, transform.position) <= exist_scope;
    }

    GameObject new_tree() {
        GameObject tree = Instantiate(tree_prefabs[Random.Range(0, tree_prefabs.Length)]);
        tree.transform.position = transform.position;
        Vector3 offset = random_offset();
        Vector3 pos = new Vector3(tree.transform.position.x + offset.x, 0, tree.transform.position.z + offset.z);
        if (TerrainGenerator.is_initial)
        {
            pos.y = TerrainGenerator.getHeightWithBais(pos.x, pos.y);
        }
        pos.y += 5.0f;
        tree.transform.position = pos;
        tree.transform.localScale = new Vector3(5,5,1);
        return tree;
    }

    void update_trees() {
        for (int i = 0; i < tree_amount; i++)
        {
            if (!inside_exist_scope(instance[i].transform.position)) {
                Destroy(instance[i]);
                instance[i] = new_tree();
            }
        }
    }
    IEnumerator Start()
    {
        instance = new List<GameObject>();
        for (int i = 0; i < tree_amount; i++) {
            instance.Add(new_tree());
        }
        while (true)
        {
            yield return new WaitForSeconds(3);
            update_trees();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
