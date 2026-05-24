using System.Reflection;
using NetArchTest.Rules;
using NetSdrClientApp;
using NetSdrClientApp.Messages;
using NetSdrClientApp.Networking;

namespace NetSdrClientAppTests;

public class ArchitectureTests
{
    private static readonly Assembly AppAssembly = typeof(NetSdrClient).Assembly;

    // Rule 1: Messages layer must not reference Networking layer.
    // Messages encode the NetSDR protocol and should be transport-agnostic.
    [Test]
    public void Messages_Should_Not_Depend_On_Networking()
    {
        var result = Types.InAssembly(AppAssembly)
            .That().ResideInNamespace("NetSdrClientApp.Messages")
            .ShouldNot().HaveDependencyOn("NetSdrClientApp.Networking")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "Failing types: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // Rule 2: Networking layer must not reference Messages layer.
    // TcpClientWrapper / UdpClientWrapper are generic transports — they must
    // remain unaware of the NetSDR message format.
    [Test]
    public void Networking_Should_Not_Depend_On_Messages()
    {
        var result = Types.InAssembly(AppAssembly)
            .That().ResideInNamespace("NetSdrClientApp.Networking")
            .ShouldNot().HaveDependencyOn("NetSdrClientApp.Messages")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "Failing types: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // Rule 3: All interfaces in the Networking namespace must start with 'I'.
    [Test]
    public void Networking_Interfaces_Should_Start_With_I()
    {
        var result = Types.InAssembly(AppAssembly)
            .That().ResideInNamespace("NetSdrClientApp.Networking")
            .And().AreInterfaces()
            .Should().HaveNameStartingWith("I")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "Failing types: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // Rule 4: All types in the Networking namespace must be public.
    // They are part of the public contract injected into NetSdrClient.
    [Test]
    public void Networking_Types_Should_Be_Public()
    {
        var result = Types.InAssembly(AppAssembly)
            .That().ResideInNamespace("NetSdrClientApp.Networking")
            .Should().BePublic()
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True,
            "Failing types: " + string.Join(", ", result.FailingTypeNames ?? []));
    }
}
