using UnityEngine;

public class MazeCell : MonoBehaviour
{
    [SerializeField]
    private GameObject _leftWall;
    
    [SerializeField]
    private GameObject _rightWall;
    
    [SerializeField]
    private GameObject _frontWall;
    
    [SerializeField]
    private GameObject _backWall;
    
    [SerializeField]
    private GameObject _unvisitedBlock;
    
    public bool IsVisited { get; private set; }

    public void Visit()
    {
        IsVisited = true;
        if (_unvisitedBlock != null)
        {
            _unvisitedBlock.SetActive(false);
        }
    }
    
    public void ClearLeftWall()
    {
        if (_leftWall != null) 
        {
            Destroy(_leftWall);
        }
    }

    public void ClearRightWall()
    {
        if (_rightWall != null) 
        {
            Destroy(_rightWall);
        }
    }

    public void ClearFrontWall()
    {
        if (_frontWall != null) 
        {
            Destroy(_frontWall);
        }
    }

    public void ClearBackWall()
    {
        if (_backWall != null) 
        {
            Destroy(_backWall);
        }
    }
}