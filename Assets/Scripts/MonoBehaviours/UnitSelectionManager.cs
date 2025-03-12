using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
    public event EventHandler OnSelectionAreaStart;
    public event EventHandler OnSelectionAreaEnd;
    private Vector2 selectionStartMousePosition;

    public static UnitSelectionManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectionStartMousePosition = Input.mousePosition;
            OnSelectionAreaStart?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonUp(0))
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);
            var entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < entityArray.Length; i++)
            {
                entityManager.SetComponentEnabled<Selected>(entityArray[i], false);
            }
            
            
            entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>()
                .WithPresent<Selected>().Build(entityManager);
            
            entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            var localTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            
            var selectionAreaRect = GetSelectionAreaRect();
            
            for (int i = 0; i < localTransformArray.Length; i++)
            {
                LocalTransform unitLocalTransform = localTransformArray[i];
                Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint(unitLocalTransform.Position);

                if (selectionAreaRect.Contains(unitScreenPosition))
                {
                    entityManager.SetComponentEnabled<Selected>(entityArray[i], true);
                }

            }

            OnSelectionAreaEnd?.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonDown(1))
        {
            var mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover, Selected>()
                .Build(entityManager);

            var unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);

            for (int i = 0; i < unitMoverArray.Length; i++)
            {
                var unitMover = unitMoverArray[i];
                unitMover.targetPosition = mouseWorldPosition;
                unitMoverArray[i] = unitMover;
            }
            entityQuery.CopyFromComponentDataArray(unitMoverArray);
        }
    }

    public Rect GetSelectionAreaRect()
    {
        Vector2 selectionEndMousePosition = Input.mousePosition;

        Vector2 lowerLeftCorner = new Vector2(
            Mathf.Min(selectionStartMousePosition.x, selectionEndMousePosition.x),
            Mathf.Min(selectionStartMousePosition.y, selectionEndMousePosition.y));
        Vector2 upperRightCorner = new Vector2(
            Mathf.Max(selectionStartMousePosition.x, selectionEndMousePosition.x),
            Mathf.Max(selectionStartMousePosition.y, selectionEndMousePosition.y));

        return new Rect(lowerLeftCorner.x, lowerLeftCorner.y, upperRightCorner.x - lowerLeftCorner.x,
            upperRightCorner.y - lowerLeftCorner.y);
    }
}