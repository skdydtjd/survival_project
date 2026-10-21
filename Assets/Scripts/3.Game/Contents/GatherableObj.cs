using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
public enum ToolType
{
    None,
    Axe,
    Lighter,
}
public interface IGatherable : IInteractable, IHoverable
{
    void CancelGathering();
}
public class GatherableObj : InteractableObj, IGatherable
{
    private readonly WorkReservation _reservation = new WorkReservation();

    [Tooltip("item id to gather")]
    [SerializeField] string itemId;
    [SerializeField] float interactableDistance;
    [SerializeField] Transform[] gatherPoint;
    [SerializeField] protected float interactionTime = 3f;
    [SerializeField] bool isSingleUse = true;
    [SerializeField] string animationName = "Logging";

    [SerializeField] bool isGatherable = true;
    protected bool isInteracting = false;
    bool isClose = false;

    [Header("Temp")]
    [SerializeField] GameObject character;
    SpriteRenderer spriteRenderer;
    Color outlineColor;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer == null) { spriteRenderer = GetComponentInChildren<SpriteRenderer>(); }
    }
    protected virtual void Start()
    {
        outlineColor = spriteRenderer.material.GetColor("_SolidOutline");
    }
    protected virtual void Update()
    {
        if (Character.Instance == null)
            isClose = Vector3.Distance(character.transform.position, transform.position) < interactableDistance;
        else isClose = Vector3.Distance(Character.Instance.transform.position, transform.position) < interactableDistance;

    }
    public override void InteractionLeftButtonFuc(GameObject hitObject)
    {
        BeginInteraction();
    }

    public override void BeginInteraction()
    {
        if (!isGatherable)
            return;

        if (isInteracting)
            return;

        if (!CanStartInteraction())
            return;

        isInteracting = true;

        InteractionData data = new()
        {
            MovePosition = gatherPoint[0].position,
            InteractionTime = interactionTime,
            AnimationName = animationName
        };

        character
            .GetComponent<TempPlayer>()
            .StartInteraction(data, this);
    }
    public override void EndInteraction()
    {
        OnInteractionFinished();

        isInteracting = false;
    }
    protected virtual void OnInteractionFinished()
    {
        GetItem(itemId);

        if (isSingleUse)
        {
            isGatherable = false;
            gameObject.SetActive(false);
        }
        else
        {
            ChangeGatherState();
        }
    }


    public override void OnHoverEnter()
    {
        if (!isClose)
            return;

        if (!isGatherable)
            return;

        if (isInteracting)
            return;

        outlineColor.a = 1;

        spriteRenderer.material.SetColor("_SolidOutline", outlineColor);

        FloatingText.Instance.Show(name);
    }

    public override void OnHoverExit()
    {
        FloatingText.Instance.Hide();

        outlineColor.a = 0;

        spriteRenderer.material.SetColor("_SolidOutline", outlineColor);

    }

    protected virtual void ChangeGatherState()
    {
        isGatherable = false;
    }
    protected void GetItem(string itemId)
    {
        Debug.Log($"Get Item : {itemId}");
    }
    protected virtual bool CanStartInteraction()
    {
        return true;
    }

    public void CancelGathering()
    {
        isInteracting = false;
    }

    public bool CanBeGatheredBy(SkullController skull)
    {
        return skull != null && isGatherable && !isInteracting;
    }

    public void ExecuteGatherBySkull(SkullController skull, ToolType toolType)
    {
        if (!CanBeGatheredBy(skull))
            return;

        GetItem(itemId);

        if (isSingleUse)
        {
            isGatherable = false;
            gameObject.SetActive(false);
        }
        else
        {
            ChangeGatherState();
        }
    }

    public bool TryReserve(SkullController skull)
    {
        return CanBeGatheredBy(skull)
            && _reservation.TryReserve(skull);
    }

    public bool IsReservedBy(SkullController skull)
    {
        return _reservation.IsReservedBy(skull);
    }

    public void Release(SkullController skull)
    {
        _reservation.Release(skull);
    }
}
