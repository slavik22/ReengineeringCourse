using NetSdrClientApp.Networking;

namespace NetSdrClientAppTests;

[TestFixture]
public class UdpClientWrapperTests
{
    [Test]
    public void GetHashCode_IsConsistentAcrossCalls()
    {
        var wrapper = new UdpClientWrapper(12399);
        var hash = wrapper.GetHashCode();
        Assert.That(wrapper.GetHashCode(), Is.EqualTo(hash));
    }

    [Test]
    public void GetHashCode_DifferentPorts_ReturnDifferentValues()
    {
        var w1 = new UdpClientWrapper(12399);
        var w2 = new UdpClientWrapper(12400);
        Assert.That(w1.GetHashCode(), Is.Not.EqualTo(w2.GetHashCode()));
    }

    [Test]
    public void Equals_SamePort_ReturnsTrue()
    {
        var w1 = new UdpClientWrapper(12399);
        var w2 = new UdpClientWrapper(12399);
        Assert.That(w1, Is.EqualTo(w2));
    }

    [Test]
    public void Equals_DifferentPort_ReturnsFalse()
    {
        var w1 = new UdpClientWrapper(12399);
        var w2 = new UdpClientWrapper(12400);
        Assert.That(w1, Is.Not.EqualTo(w2));
    }

    [Test]
    public void Equals_Null_ReturnsFalse()
    {
        var wrapper = new UdpClientWrapper(12399);
        Assert.That(wrapper, Is.Not.EqualTo(null));
    }

    [Test]
    public void Equals_DifferentType_ReturnsFalse()
    {
        var wrapper = new UdpClientWrapper(12399);
        Assert.That(wrapper, Is.Not.EqualTo(new object()));
    }
}
