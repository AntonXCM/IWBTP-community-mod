using UnityEngine;
using UnityEngine.UI;

public class ParableNarrator : MonoBehaviour
{
	public Text Subtitles;
	public new CameraFollow camera;

	[SerializeField] RectTransform
		layoutTop,
		layoutTopLeft,
		layoutBottom,
		layoutCenterToLeft,
		layoutLeftSide,
		layoutTopRight;
	public void Start()
	{
		camera = FindAnyObjectByType<CameraFollow>();
		camera.OnMove += OnMove;
	}
	public void OnMove()
	{
		Subtitles.text = "";
		Subtitles.alignment = GetAlignment();
		Subtitles.rectTransform.SetParent(GetLayout(), false);
	}
	public Color GetColor()
	{
		switch(camera.CurrentScreen.x, camera.CurrentScreen.y)
		{
			case (0,4):
			Subtitles.text = "Ты вышел на прогулку в игру, а не на улицу?\nЧел, это странно, очень странно, я бы так никогда не сделал.\n ";
			return Color.black;
			default:
			return Color.white;
		}
	}
	public TextAnchor GetAlignment()
	{
		switch(camera.CurrentScreen.x, camera.CurrentScreen.y)
		{
			case (0, 2):
			return TextAnchor.UpperLeft;
			default:
			return TextAnchor.MiddleCenter;
		}
	}
	public RectTransform GetLayout()
	{
		switch(camera.CurrentScreen.x, camera.CurrentScreen.y)
		{
			case (0, 1):
			return layoutCenterToLeft;
			case (0, -1):
			return layoutBottom;
			case (0, 2):
			return layoutTopLeft;
			case (0, 4):
			return layoutTopRight;
			case (0, 0):
			default:
			return layoutTop;
		}
	}
}