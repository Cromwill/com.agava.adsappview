using System.Collections;
using AdsAppView.Program;
using AdsAppView.Utility;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ViewPresenterConfigTests
{
    private AdsAppAPI _api;
    private ViewPresenterConfigs _viewPresenterConfigs;

    [SetUp]
    public void Setup()
    {
        _viewPresenterConfigs = new GameObject("ViewPresenterConfigs").AddComponent<ViewPresenterConfigs>();
        _api = new("gamesrtkbd.tech", Application.identifier);
    }

    [UnityTest]
    public IEnumerator Initialize_ConfigsSet_ViewPresenterTypeIsNotDefault()
    {
        yield return _viewPresenterConfigs.Initialize(_api);

        Assert.AreNotEqual(ViewPresenterConfigs.ViewPresenterType, ViewPresenterConfigs.DefaultViewPresenterType);
    }
}
