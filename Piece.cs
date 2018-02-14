using System.Collections;
using UnityEngine;

public class Piece : MonoBehaviour
{
    private int id = -1;
    private PlayerType owner = PlayerType.Player1;
    private bool initialized = false;
    private bool scored;
    private Coroutine movement;

    Transform basePieceHolder;

    // Models
    [SerializeField]
    Transform characterHolder;
    [SerializeField]
    GameObject character1Prefab;
    [SerializeField]
    GameObject character2Prefab;
    
    Animator anim;
    

    // Init
    public void Initialize(int id, PlayerType owner)
    {
        if (!initialized)
        {
            initialized = true;

            this.id = id;
            this.owner = owner;

            // Update ID Text to proper local id
            TextMesh pieceText = transform.FindChild("ID Text").GetComponent<TextMesh>();
            pieceText.text = id.ToString();
            pieceText.color = (owner == PlayerType.Player1) ? Color.red : Color.blue;
            pieceText.gameObject.SetActive(false);
        }
        else
            Debug.LogError("Piece already initialized!");
    }

    private void Start()
    {
        if(owner == PlayerType.Player1)
            anim = Instantiate(character1Prefab, characterHolder.transform.position, characterHolder.transform.rotation, characterHolder).GetComponent<Animator>();
        else
            anim = Instantiate(character2Prefab, characterHolder.transform.position, characterHolder.transform.rotation, characterHolder).GetComponent<Animator>();

        basePieceHolder = transform.parent;

        // Move this Piece to the proper location in storage
        Vector3 pos = (id <= 3) ? transform.parent.position + Vector3.forward * id : transform.parent.position + Vector3.forward * (id - 3) + Vector3.right * Mathf.Sign(transform.parent.position.x);
        MoveTo(pos);
    }

    // Tile object
    public void MoveTo(Tile target)
    {
        if (movement != null)
            StopCoroutine(movement);

        if (target != null)
            movement = StartCoroutine(MoveToPosition(target.GetObjectPosition()));
        else
        {
            // Move this Piece to the proper location in storage
            Vector3 pos = (id <= 3) ? transform.parent.position + Vector3.forward * id : transform.parent.position + Vector3.forward * (id - 3) + Vector3.right * Mathf.Sign(transform.parent.position.x);
            MoveTo(pos);
        }
    }

    // Vector3
    public void MoveTo(Vector3 target)
    {
        if(movement != null)
            StopCoroutine(movement);

        movement = StartCoroutine(MoveToPosition(target));
    }

    #region Helper Functions

    public void SetPieceID(int newId)
    {
        id = newId;
        transform.FindChild("ID Text").GetComponent<TextMesh>().text = id.ToString();
    }

    public int GetPieceID()
    {
        return id;
    }

    public void SetOwner(PlayerType owner)
    {
        this.owner = owner;
    }

    public PlayerType GetOwner()
    {
        return owner;
    }

    IEnumerator MoveToPosition(Vector3 target)
    {
        float dist = Vector3.Distance(transform.position, target);

        while (dist > 0.025f)
        {
            // Move to target
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * 3f);
            dist = Vector3.Distance(transform.position, target);

            if (anim.isInitialized)
                anim.SetBool("IsMoving", true);

            // Face target
            Vector3 lookVector = target - transform.position;
            lookVector.y = 0;
            if(lookVector != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookVector);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
            }
            yield return null;
        }

        anim.SetBool("IsMoving", false);
    }

    public void FaceTile(Tile tile)
    {
        StartCoroutine(FaceTileCoroutine(tile.GetObjectPosition()));
    }

    IEnumerator FaceTileCoroutine(Vector3 target)
    {
        StopCoroutine(movement);

        // Face target
        Vector3 lookVector = target - transform.position;
        lookVector.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(lookVector);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.05f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
            targetRotation = Quaternion.LookRotation(lookVector);
            yield return null;
        }
    }

    public void Attack()
    {
        StartCoroutine(AttackAnimation());
    }

    IEnumerator AttackAnimation()
    {
        yield return new WaitForSeconds(0.1f);
        anim.SetBool("IsMoving", false);
        anim.SetInteger("AttackType", Random.Range(0, 2));
        anim.SetBool("IsAttacking", true);

        yield return new WaitForSeconds(1);
        anim.SetBool("IsAttacking", false);
    }

    public void Die()
    {
        transform.SetParent(basePieceHolder);

        StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        anim.SetInteger("DeathType", Random.Range(0, 2));
        anim.SetBool("IsDying", true);
        yield return new WaitForSeconds(3f);
        anim.SetBool("IsDying", false);

        yield return new WaitForSeconds(0.5f);
        // Turn invisible
        characterHolder.gameObject.SetActive(false);
        // Move back to our storage
        MoveTo(null);

        yield return new WaitForSeconds(1.7f);
        // Turn visible again
        characterHolder.gameObject.SetActive(true);
    }

    public override string ToString()
    {
        return owner.ToString() + " piece " + id;
    }

    public bool Scored
    {
        get
        {
            return scored;
        }
        set
        {
            scored = value;
        }
    }

    #endregion
}
