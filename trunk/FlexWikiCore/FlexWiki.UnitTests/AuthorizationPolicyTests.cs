#region License Statement
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion

using System;
using System.Security.Principal;
using System.Threading;

using NUnit.Framework;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    [Ignore("This test disabled during the 2.0 upgrade. Re-enable as functionality is implemented.")]
    public class AuthorizationPolicyTests
    {
        private Federation _federation;

        [SetUp]
        public void SetUp()
        {
            /* Set up a mock configuration with the following permissions: 
             * 
             * Wiki-wide: 
             * Alice: read, write, administer
             * Bob: read
             * 
             * NamespaceOne: 
             * Alice: read
             * 
             * NamespaceTwo
             * Bob: read, write, administer
             */
            MockWikiApplication application = new MockWikiApplication(
                null,
                null,
                OutputFormat.HTML,
                new MockTimeProvider(TimeSpan.FromSeconds(1))); 

            _federation = new Federation(application);

            MockAuthorizationConfigurationProvider configProvider = new MockAuthorizationConfigurationProvider();
            configProvider.AddWikiPermission("Alice", Permission.Read);
            configProvider.AddWikiPermission("Alice", Permission.Edit);
            configProvider.AddWikiPermission("Alice", Permission.Administer);
            configProvider.AddWikiPermission("Bob", Permission.Read);
            configProvider.AddWikiPermission("WikiReaders", Permission.Read);
            configProvider.AddWikiPermission("WikiEditors", Permission.Edit);
            configProvider.AddWikiPermission("WikiAdmins", Permission.Administer);

            configProvider.AddNamespacePermission("Alice", "NamespaceOne", Permission.Read);

            configProvider.AddNamespacePermission("Bob", "NamespaceTwo", Permission.Read);
            configProvider.AddNamespacePermission("Bob", "NamespaceTwo", Permission.Edit);
            configProvider.AddNamespacePermission("Bob", "NamespaceTwo", Permission.Administer);

            configProvider.AddNamespacePermission("NS4Readers", "NamespaceFour", Permission.Read);
            configProvider.AddNamespacePermission("NS4Editors", "NamespaceFour", Permission.Edit);
            configProvider.AddNamespacePermission("NS4Admins", "NamespaceFour", Permission.Administer);

            configProvider.AddNamespacePermission("*", "NamespaceFive", Permission.Read);
            configProvider.AddNamespacePermission("?", "NamespaceFive", Permission.Edit);

            _federation.AuthorizationConfigurationProvider = configProvider;
        }

        [Test]
        public void AdministerImpliesReadAndEdit()
        {
            AssertPermission("NamespaceFour", Permission.Read, true, "Alice", "NS4Admins");
            AssertPermission("NamespaceFour", Permission.Edit, true, "Alice", "NS4Admins");
        }

        [Test]
        public void AdministerRole()
        {
            // Allowed by explicit namespace permissions
            AssertPermission("NamespaceFour", Permission.Administer, true, "Dale", "NS4Admins");

            // Not allowed by explicit namespace permissions
            AssertPermission("NamespaceFour", Permission.Administer, false, "Dale", "NS4Readers");

            // Allowed by wiki permissions
            AssertPermission("NamespaceThree", Permission.Administer, true, "Dale", "WikiAdmins");

            // Not allowed by wiki permissions
            AssertPermission("NamespaceFour", Permission.Administer, false, "Dale", "WikiReaders");
        }

        [Test]
        public void AdministerUser()
        {
            // Allowed by explicit namespace permissions
            AssertPermission("NamespaceTwo", Permission.Administer, true, "Bob");

            // Not allowed by explicit namespace permissions
            AssertPermission("NamespaceTwo", Permission.Administer, false, "Alice");

            // Allowed by wiki permissions
            AssertPermission("NamespaceThree", Permission.Administer, true, "Alice");

            // Not allowed by wiki permissions
            AssertPermission("NamespaceThree", Permission.Administer, false, "Bob");

        }

        [Test]
        public void AnonymousAccess()
        {
            // Check that anonymous user is allowed read access
            AssertPermission("NamespaceFive", Permission.Read, true, new MockAnonymousPrincipal());

            // Check that anonymous user is denied edit access
            AssertPermission("NamespaceFive", Permission.Edit, false, new MockAnonymousPrincipal());

        }

        [Test]
        public void AuthenticatedUserHasAnonymousAccess()
        {
            // Authenticated users should still have access to namespaces that allow unauthenticated access
            AssertPermission("NamespaceFive", Permission.Read, true, "Alice");

            // Authenticated users should not have admin access in namespaces that allow authenticated edits
            AssertPermission("NamespaceFive", Permission.Administer, false, "Alice");
        }

        [Test]
        public void AuthenticatedAccess()
        {
            // Check that any authenticated user is allowed edit access
            AssertPermission("NamespaceFive", Permission.Edit, true, "Evelyn");

            // Check that any authenticated user is denied admin access
            AssertPermission("NamespaceFive", Permission.Administer, false, "Evelyn");

            // Check that anonymous user is denied edit access
            AssertPermission("NamespaceFive", Permission.Edit, false, new MockAnonymousPrincipal());
        }

        [Test]
        public void EditImpliesRead()
        {
            AssertPermission("NamespaceFour", Permission.Read, true, "Alice", "NS4Editors");
        }

        [Test]
        public void EditRole()
        {
            // Allowed by explicit namespace permissions
            AssertPermission("NamespaceFour", Permission.Edit, true, "Dale", "NS4Editors");

            // Not allowed by explicit namespace permissions
            AssertPermission("NamespaceFour", Permission.Edit, false, "Dale", "NS4Readers");

            // Allowed by wiki permissions
            AssertPermission("NamespaceThree", Permission.Edit, true, "Dale", "WikiEditors");

            // Not allowed by wiki permissions
            AssertPermission("NamespaceFour", Permission.Edit, false, "Dale", "WikiReaders");

        }
        [Test]
        public void EditUser()
        {
            // Allowed by explicit namespace permissions
            AssertPermission("NamespaceTwo", Permission.Edit, true, "Bob");

            // Not allowed by explicit namespace permissions
            AssertPermission("NamespaceTwo", Permission.Edit, false, "Alice");

            // Allowed by wiki permissions
            AssertPermission("NamespaceThree", Permission.Edit, true, "Alice");

            // Not allowed by wiki permissions
            AssertPermission("NamespaceThree", Permission.Edit, false, "Bob");

        }

        [Test]
        public void NullNamespace()
        {
            // Check that passing null for a namespace doesnm't break the app, but
            // returns false. 
            AssertPermission(null, Permission.Read, false, "Alice");
        }
        [Test]
        public void ReadRole()
        {
            // Allowed by explicit namespace permissions
            AssertPermission("NamespaceFour", Permission.Read, true, "Dale", "NS4Readers");

            // Not allowed by explicit namespace permissions
            AssertPermission("NamespaceFour", Permission.Read, false, "Dale");

            // Allowed by wiki permissions
            AssertPermission("NamespaceThree", Permission.Read, true, "Dale", "WikiReaders");

            // Not allowed by wiki permissions
            AssertPermission("NamespaceFour", Permission.Read, false, "Dale", "Blurg");

        }
        [Test]
        public void ReadUser()
        {
            // Allowed by explicit namespace permissions
            AssertPermission("NamespaceOne", Permission.Read, true, "Alice");

            // Not allowed by explicit namespace permissions
            AssertPermission("NamespaceTwo", Permission.Read, false, "Alice");

            // Allowed by wiki permissions
            AssertPermission("NamespaceThree", Permission.Read, true, "Alice");

            // Not allowed by wiki permissions
            AssertPermission("NamespaceThree", Permission.Read, false, "Charlie");

        }

        [Test]
        public void UsernamesAndRolesAreCaseInsensitive()
        {
            AssertPermission("NamespaceOne", Permission.Read, true, "alice");
            AssertPermission("NamespaceFour", Permission.Read, true, "Alice", "ns4readers");
        }


        private void AssertPermission(string nmspc, Permission permission, bool shouldHave, string username, params string[] roles)
        {
            AssertPermission(nmspc, permission, shouldHave, new GenericPrincipal(new GenericIdentity(username), roles));
        }

        private void AssertPermission(string nmspc, Permission permission, bool shouldHave, IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;

            if (shouldHave)
            {
                string message = string.Format("Checking that user '{0}' has permission {1} on namespace {2}", principal.Identity.Name, permission, nmspc);
                Assert.IsTrue(_federation.HasPermission(nmspc, permission), message);
            }
            else
            {
                string message = string.Format("Checking that user '{0}' DOES NOT HAVE permission {1} on namespace {2}", principal.Identity.Name, permission, nmspc);
                Assert.IsFalse(_federation.HasPermission(nmspc, permission), message);
            }
        }


    }
}
