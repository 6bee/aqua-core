// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Tests.EnumerableExtensions;

using Aqua.EnumerableExtensions;
using Shouldly;
using System.Linq;
using Xunit;

public class When_using_outer_join
{
    [Fact]
    public void Full_outer_join()
    {
        var list1 = new int?[] { 1, 2, 3 };
        var list2 = new int?[] { 3, 4, 5 };

        var jointList = list1.FullOuterJoin(list2, x => x);

        jointList.Count().ShouldBe(5);

        jointList.ElementAt(0).Left.ShouldBe(1);
        jointList.ElementAt(0).Right.ShouldBeNull();

        jointList.ElementAt(1).Left.ShouldBe(2);
        jointList.ElementAt(1).Right.ShouldBeNull();

        jointList.ElementAt(2).Left.ShouldBe(3);
        jointList.ElementAt(2).Right.ShouldBe(3);

        jointList.ElementAt(3).Left.ShouldBeNull();
        jointList.ElementAt(3).Right.ShouldBe(4);

        jointList.ElementAt(4).Left.ShouldBeNull();
        jointList.ElementAt(4).Right.ShouldBe(5);
    }

    [Fact]
    public void Full_outer_join_with_custom_result_type()
    {
        var list1 = new int?[] { 1, 2, 3 };
        var list2 = new int?[] { 3, 4, 5 };

        var jointList = list1.FullOuterJoin(list2, x => x, (a, b) => new { L = a, R = b });

        jointList.Count().ShouldBe(5);

        jointList.ElementAt(0).L.ShouldBe(1);
        jointList.ElementAt(0).R.ShouldBeNull();

        jointList.ElementAt(1).L.ShouldBe(2);
        jointList.ElementAt(1).R.ShouldBeNull();

        jointList.ElementAt(2).L.ShouldBe(3);
        jointList.ElementAt(2).R.ShouldBe(3);

        jointList.ElementAt(3).L.ShouldBeNull();
        jointList.ElementAt(3).R.ShouldBe(4);

        jointList.ElementAt(4).L.ShouldBeNull();
        jointList.ElementAt(4).R.ShouldBe(5);
    }

    [Fact]
    public void Right_outer_join()
    {
        var list1 = new int?[] { 1, 2, 3 };
        var list2 = new int?[] { 3, 4, 5 };

        var jointList = list1.RightOuterJoin(list2, x => x);

        jointList.Count().ShouldBe(3);

        jointList.ElementAt(0).Left.ShouldBe(3);
        jointList.ElementAt(0).Right.ShouldBe(3);

        jointList.ElementAt(1).Left.ShouldBeNull();
        jointList.ElementAt(1).Right.ShouldBe(4);

        jointList.ElementAt(2).Left.ShouldBeNull();
        jointList.ElementAt(2).Right.ShouldBe(5);
    }

    [Fact]
    public void Right_outer_join_with_custom_result_type()
    {
        var list1 = new int?[] { 1, 2, 3 };
        var list2 = new int?[] { 3, 4, 5 };

        var jointList = list1.RightOuterJoin(list2, x => x, (a, b) => new { L = a, R = b });

        jointList.Count().ShouldBe(3);

        jointList.ElementAt(0).L.ShouldBe(3);
        jointList.ElementAt(0).R.ShouldBe(3);

        jointList.ElementAt(1).L.ShouldBeNull();
        jointList.ElementAt(1).R.ShouldBe(4);

        jointList.ElementAt(2).L.ShouldBeNull();
        jointList.ElementAt(2).R.ShouldBe(5);
    }

    [Fact]
    public void Left_outer_join()
    {
        var list1 = new int?[] { 1, 2, 3 };
        var list2 = new int?[] { 3, 4, 5 };

        var jointList = list1.LeftOuterJoin(list2, x => x);

        jointList.Count().ShouldBe(3);

        jointList.ElementAt(0).Left.ShouldBe(1);
        jointList.ElementAt(0).Right.ShouldBeNull();

        jointList.ElementAt(1).Left.ShouldBe(2);
        jointList.ElementAt(1).Right.ShouldBeNull();

        jointList.ElementAt(2).Left.ShouldBe(3);
        jointList.ElementAt(2).Right.ShouldBe(3);
    }

    [Fact]
    public void Left_outer_join_with_custom_result_type()
    {
        var list1 = new int?[] { 1, 2, 3 };
        var list2 = new int?[] { 3, 4, 5 };

        var jointList = list1.LeftOuterJoin(list2, x => x, (a, b) => new { L = a, R = b });

        jointList.Count().ShouldBe(3);

        jointList.ElementAt(0).L.ShouldBe(1);
        jointList.ElementAt(0).R.ShouldBeNull();

        jointList.ElementAt(1).L.ShouldBe(2);
        jointList.ElementAt(1).R.ShouldBeNull();

        jointList.ElementAt(2).L.ShouldBe(3);
        jointList.ElementAt(2).R.ShouldBe(3);
    }
}