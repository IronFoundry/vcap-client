﻿
using System.IO;
using FluentAssertions;

namespace IronFoundry.VcapClient.IntegrationTests
{
    using NUnit.Framework;

    [TestFixture]
    public class when_managing_applications
    {
        protected VcapClient CloudActive;

        [TestFixtureSetUp]
        public void with_known_good_cloud_target()
        {
            CloudActive = new VcapClient(TestAccountInformation.GoodUri.ToString());
            CloudActive.Login(
                TestAccountInformation.Username,
                TestAccountInformation.Password);
        }

        [TestFixtureTearDown]
        public void cleaning_up_the_testing()
        {

        }

        [Test]
        public void should_detect_known_apps()
        {
            var apps = CloudActive.GetApplications();
            apps.Should().NotBeNull();
        }

   
    }
}
