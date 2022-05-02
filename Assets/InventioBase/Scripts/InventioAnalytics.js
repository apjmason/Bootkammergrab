#pragma strict

public var useAnalytics : boolean = true;
public var analyticsServiceClassName : String = null;
public var analyticsKey : String = null;
public var miscString : String = null;

public var locationTrackingApplicationId : String = null;
public var locationTrackingClientId : String = null;
public var locationTrackingInterval : int = 30; // [s]

function Start () {
	if (useAnalytics && analyticsServiceClassName && analyticsKey) {
		/*
		Can't activate right away sine the UIActionSheet
		shown to the user won't be displayed correctly.
		*/
		Invoke("ActivateAnalytics", 5);
	}
}

function ActivateAnalytics()
{
	InventioAnalyticsManager.activateAnalytics(analyticsServiceClassName, analyticsKey, miscString);
	if (locationTrackingApplicationId && locationTrackingClientId) {
		Debug.LogError("TrackerNG not supported in JavaScript!");
	}
	gameObject.SetActive(false);
}