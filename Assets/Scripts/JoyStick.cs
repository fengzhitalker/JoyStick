using System.Collections;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoyStick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image controller;
    public float ResetTime = 1f;
    public Rect JoyStickArea = new Rect(0,0,100,100);
    
    private Vector2 _originControllerPos;
    private float _maxDragLength;
    private float _maxDragSqrLength;
    
    // 复位相关变量
    private float _hasResetTime;
    private Coroutine _resetCoroutine;
    private bool _isDrag;
    private bool _isPointDown;
    
    // 移动速度变量
    private float _hAxis;
    private float _vAxis;
    
    private void Awake()
    {
        this._originControllerPos = controller.rectTransform.position;
        this._maxDragLength = JoyStickArea.width;
        this._maxDragSqrLength = this._maxDragLength * this._maxDragLength;
    }

    // 通过键盘获取移动速度
    private void Update()
    {
        // 拖拽状态中，则不通过键盘获取
        if (_isDrag || _isPointDown)
        {
            return;
        }

        var hAxis = Input.GetAxis("Horizontal");
        var vAxis = Input.GetAxis("Vertical");
        
        var axis = new Vector2(hAxis, vAxis);
        if (axis.sqrMagnitude > 1)
        {
            axis = axis.normalized;
        }
        
        SetControllerPos(axis * this._maxDragLength);
    }

    public float GetHAxis()
    {
        return _hAxis;
    }

    public float GetVAxis()
    {
        return _vAxis;
    }

    private bool CheckInJoyStickArea(Vector2 pos)
    {
        return JoyStickArea.Contains(pos);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("point down");
        _isPointDown = true;
        StopResetCoroutine();
        SetControllerPos(eventData.position - _originControllerPos);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("point up");
        _isPointDown = false;
        if (_isDrag)
            return;
        
        EndMove(eventData.position);
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("begin drag");
        _isDrag = true;
        StartMove(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("on drag");
        Vector2 diffPos = eventData.position - _originControllerPos;
        if (diffPos.sqrMagnitude > this._maxDragSqrLength)
        {
            Debug.LogFormat("too long {0}", eventData.position);
            diffPos = diffPos.normalized * this._maxDragLength;
        }

        SetControllerPos(diffPos);        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("end drag");
        _hAxis = 0;
        _vAxis = 0;
        _isDrag = false;
        EndMove(eventData.position);
    }

    private void SetControllerPos(Vector2 diffPos)
    {
        _hAxis = diffPos.x / this._maxDragLength;
        _vAxis = diffPos.y / this._maxDragLength;
        controller.rectTransform.position = _originControllerPos + diffPos;
    }

    private void StartMove(Vector3 pos)
    {
        StopResetCoroutine();
    }

    private void StopResetCoroutine()
    {
        if (this._resetCoroutine != null)
        {
            StopCoroutine(this._resetCoroutine);
            this._resetCoroutine = null;
        }
    }
    
    private void EndMove(Vector3 pos)
    {
        this._hasResetTime = 0;
        this._resetCoroutine = StartCoroutine(ResetController());
    }
    
    // 将控制块复位
    private IEnumerator ResetController()
    {
        var curPos = controller.rectTransform.position;

        while (this._hasResetTime < ResetTime)
        {
            this._hasResetTime += Time.deltaTime;

            controller.rectTransform.position = Vector3.Lerp(
                curPos, _originControllerPos, this._hasResetTime / ResetTime);

            yield return 0;
        }
    }
}