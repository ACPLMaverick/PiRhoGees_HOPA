using UnityEngine;
using System.Collections;

public class FloatingParticleManager : Singleton<FloatingParticleManager>
{
    #region fields
    [SerializeField]
    private float _radiusMin = 50.0f;
    [SerializeField]
    private float _radiusMax = 1.0f;
    #endregion

    #region private

    private ParticleSystem _particles;
    private GameObject _source;
    private GameObject _target;

    #endregion

    #region functions
    // Use this for initialization
    protected override void Awake()
    {
        base.Awake();
        _particles = GetComponent<ParticleSystem>();
        gameObject.SetActive(false);
        _particles.Stop();
    }

    void Start ()
    {

	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 sPos = _source.transform.position;
        Vector3 tPos = _target.transform.position;
        tPos = Camera.main.WorldToScreenPoint(tPos);
        float dist = Vector3.Distance(sPos, tPos);
        dist = Mathf.Clamp(dist, _radiusMax, _radiusMin);
        float lerp = Mathf.Clamp01((_radiusMin - dist) / (_radiusMin));
        _particles.playbackSpeed = Mathf.Clamp01(lerp + 0.1f);
        Color col = _particles.startColor;
        col.a = lerp * lerp;
        _particles.startColor = col;

        sPos = Camera.main.ScreenToWorldPoint(sPos);
        sPos.z = transform.position.z;
        transform.position = sPos;

        Debug.Log(dist);
	}

    public void Show(GameObject source, GameObject target)
    {
        _source = source;
        _target = target;
        gameObject.SetActive(true);
        _particles.Play();
    }

    public void Hide()
    {
        _source = null;
        _target = null;
        gameObject.SetActive(false);
        _particles.Play();
    }

    #endregion
}
