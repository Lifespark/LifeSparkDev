using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AGE{

public class CurveContainer
{
	protected const float maxSlope = 1000.0f;

	public CurveContainer()
	{
		//curves = new List<Curve>();		
	}

	public CurveContainer(string[] _curveNames)
	{		
		foreach (string curveName in _curveNames)
			AddCurve(curveName);
	}

	public CurveContainer(float val)
	{
		Curve curve = new Curve("defaultCurveName");
		curve.AddPoint(0, val, 0, true);
		curve.AddPoint(1, val, 0, true);
		
		AddCurve(curve);
	}

	public struct Curve
	{
		public Curve(string _name)
		{
			name = _name;
			points = new List<Point>();				
		}

		public struct Point
		{
			public float time;
			public float value;
			public float slope;
		}
		public List<Point> points;
		public string name;

		float LocatePoint(float _curTime)
		{
			float result = 0.0f;
			
			int pointCount = points.Count;

			{
				if (_curTime < 0) _curTime = 0;
				else if (_curTime > 1.0f) _curTime = 1.0f;
			}
			
			int pointIndex = -1;
			
			int begin = 0;
			int end = points.Count - 1;
			
			while (begin != end)
			{
				int mid = (begin + end) / 2 + 1;
				if (_curTime < points[mid].time)
				{
					//search left branch
					end = mid - 1;
				}
				else
				{
					//search right branch
					begin = mid;
				}
			}
			
			if (begin == 0 && _curTime < points[0].time)
				pointIndex = -1;
			else
				pointIndex = begin;
			
			if (pointIndex < 0) //before the first point
			{
				{
					result = -1 + _curTime / points[0].time;
				}
			}
			else if (pointIndex == pointCount - 1) //the last point
			{
				{
						result = (pointCount - 1); // + (_curTime - points[pointCount-1].time) / (1.0f - points[pointCount-1].time);
				}
			}
			else
			{
				result = pointIndex + (_curTime - points[pointIndex].time) / (points[pointIndex+1].time - points[pointIndex].time);
			}
			return result;
		}

		public float Sample(float _curTime)
		{
			if (points.Count == 0) return 0.0f;
			float pointPos = LocatePoint(_curTime);
			if (pointPos < 0)
			{
				return points[0].value;
			}
			else if (pointPos >= points.Count - 1)
			{
				return points[points.Count-1].value;
			}
			else
			{
				int pos = (int)pointPos;
				float weight = (_curTime - points[pos].time) / (points[pos+1].time - points[pos].time);
				if (Math.Abs(points[pos].slope) < maxSlope && Math.Abs(points[pos+1].slope) < maxSlope)
				{
					float value1 = (_curTime - points[pos].time) * points[pos].slope + points[pos].value;
					float value2 = (_curTime - points[pos+1].time) * points[pos+1].slope + points[pos+1].value;
					//return value1 * (1.0f - weight) + value2 * weight;
					float ctrlSlope1 = (points[pos].slope * 2.3f + points[pos+1].slope * 1.0f) / 3.3f;
					float ctrlSlope2 = (points[pos].slope * 1.0f + points[pos+1].slope * 2.3f) / 3.3f;
					float ctrl1 = (_curTime - points[pos].time) * ctrlSlope1 + points[pos].value;
					float ctrl2 = (_curTime - points[pos+1].time) * ctrlSlope2 + points[pos+1].value;
					
					float t = weight;
					float rt = 1.0f - t;
					return value1 * rt * rt * rt +
						   ctrl1 * rt * rt * t * 3 +
						   ctrl2 * rt * t * t * 3 +
						   value2 * t * t * t;

				}
				else
				{
					return points[pos].slope;
				}
			}
		}

		public int AddPoint(float _time, float _value, float _slope, bool _addDirectly)
		{
			Point newPoint = new Point();
			newPoint.time = _time;
			newPoint.value = _value;
			newPoint.slope = _slope;

			int insertPos = 0;
			if (points.Count == 0 || _addDirectly)
			{
				points.Add(newPoint);
			}
			else
			{
				float insertPosFloat = LocatePoint(_time);
				insertPos = (int)(insertPosFloat + 1);
				if (insertPos > points.Count) insertPos = points.Count;
				points.Insert(insertPos, newPoint);
			}

			return insertPos;
		}
	}

	protected List<Curve> curves = new List<Curve>();

	public float[] SampleCurves(float _curTime)
	{
		float[] result = new float[curves.Count];
		for (int i=0; i<curves.Count; i++)
			result[i] = curves[i].Sample(_curTime);
		return result;
	}

	public void AddCurve(string _name) { curves.Add(new Curve(_name)); }
	public void AddCurve(Curve _curve)
	{
			curves.Add(_curve);
	}

	public float SampleFloat(float _time)
	{
		float[] sampleResult = SampleCurves(_time);
		return sampleResult[0];
	}

	public Vector2 SampleVector2(float _time)
	{
		float[] sampleResult = SampleCurves(_time);
		return new Vector2(sampleResult[0], sampleResult[1]);
	}

	public Vector3 SampleVector3(float _time)
	{
		float[] sampleResult = SampleCurves(_time);
		return new Vector3(sampleResult[0], sampleResult[1], sampleResult[2]);
	}

	public Quaternion SampleEulerAngle(float _time)
	{
		float[] sampleResult = SampleCurves(_time);
		return Quaternion.Euler(sampleResult[0], sampleResult[1], sampleResult[2]);
	}

	public Color SampleColor(float _time)
	{
		float[] sampleResult = SampleCurves(_time);
		return new Color(sampleResult[0], sampleResult[1], sampleResult[2], sampleResult[3]);
	}
}
}

