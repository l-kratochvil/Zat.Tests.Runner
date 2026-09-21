namespace Zat.Tests.Runner.Common.Net.TestLinkApi;

using System;

using CookComputing.XmlRpc;

public static class XmlRpcStructConvertors
{
    internal static Model.TestLinkErrorMessage ToTestLinkErrorMessage(XmlRpcStruct data)
        => new()
        {
            code = ToInt(data, "code"),
            message = (string)data["message"],
        };

    internal static Model.GeneralResult ToGeneralResult(XmlRpcStruct data)
    {
        var item = new Model.GeneralResult
        {
            operation = (string)data["operation"],
            status = (bool)data["status"],
            id = ToInt(data, "id"),
            message = (string)data["message"],
        };

        if (data.ContainsKey("additionalInfo") &&
            data["additionalInfo"] is XmlRpcStruct)
        {
            item.additionalInfo = ToAdditionalInfo(data["additionalInfo"] as XmlRpcStruct);
        }
        else
        {
            item.additionalInfo = null;
        }

        return item;
    }

    /// <summary>
    ///  constructor used by XMLRPC interface on decoding the function return
    /// </summary>
    /// <param name="data">data returned by Testlink</param>
    internal static Model.AttachmentRequestResponse ToAttachmentRequestResponse(XmlRpcStruct data)
        => new()
        {
            foreignKeyId = ToInt(data, "fk_id"),
            linkedTableName = (string)data["fk_table"],
            title = (string)data["title"],
            description = (string)data["description"],
            file_name = (string)data["file_name"],
            file_type = (string)data["file_type"],
            size = ToInt(data, "file_size")
        };

    /// <summary>
    ///  constructor used by XMLRPC interface on decoding the function return
    /// </summary>
    /// <param name="data">data returned by Testlink</param>
    internal static Model.AdditionalInfo ToAdditionalInfo(XmlRpcStruct data)
        => new()
        {
            new_name = (string)data["new_name"],
            status_ok = ToInt(data, "status_ok") == 1,
            msg = (string)data["msg"],
            id = ToInt(data, "id"),
            external_id = ToInt(data, "external_id"),
            version_number = ToInt(data, "version_number"),
            has_duplicate = ToBool(data, "has_duplicate"),
        };

    internal static Model.Build ToBuild(XmlRpcStruct data)
        => new()
        {
            id = ToInt(data, "id"),
            active = ToInt(data, "active") == 1,
            name = (string)data["name"],
            notes = (string)data["notes"],
            testplan_id = ToInt(data, "testplan_id"),
            is_open = ToInt(data, "is_open") == 1,
        };

    internal static Model.TestCaseFromTestSuite ToTestCaseFromTestSuite(XmlRpcStruct data)
    {
        var item = new Model.TestCaseFromTestSuite
        {
            active = int.Parse((string)data["active"]) == 1,
            id = ToInt(data, "id"),
            name = (string)data["name"],
            version = ToInt(data, "version"),
            tcversion_id = ToInt(data, "tcversion_id"),
            //steps = (string)data["steps"];
            //expected_results = (string)data["expected_results"];
            external_id = (string)data["tc_external_id"],
            testSuite_id = ToInt(data, "parent_id"),
            is_open = int.Parse((string)data["is_open"]) == 1,
            modification_ts = ToDate(data, "modification_ts"),
            updater_id = ToInt(data, "updater_id"),
            execution_type = ToInt(data, "execution_type"),
            summary = (string)data["summary"]
        };

        if (data.ContainsKey("details"))
        {
            item.details = (string)data["details"];
        }
        else
        {
            item.details = string.Empty;
        }

        item.author_id = ToInt(data, "author_id");
        item.creation_ts = ToDate(data, "creation_ts");
        item.importance = ToInt(data, "importance");
        item.parent_id = ToInt(data, "parent_id");
        item.node_type_id = ToInt(data, "node_type_id");
        item.node_order = ToInt(data, "node_order");
        item.node_table = (string)data["node_table"];
        item.layout = (string)data["layout"];
        item.status = ToInt(data, "status");
        item.preconditions = (string)data["preconditions"];

        return item;
    }

    /// <summary>
    ///  constructor used by the XML Rpc return
    /// </summary>
    /// <param name="data"></param>
    internal static Model.TestStep ToTestStep(XmlRpcStruct data)
        => new()
        {
            id = ToInt(data, "id"),
            step_number = ToInt(data, "step_number"),
            actions = (string)data["actions"],
            expected_results = (string)data["expected_results"],
            active = ToInt(data, "active") == 1,
            execution_type = ToInt(data, "execution_type"),
        };

    /// <summary>
    ///  constructor used by XMLRPC interface on decoding the function return
    /// </summary>
    /// <param name="data">data returned by Testlink</param>
    internal static Model.TestSuite ToTestSuite(XmlRpcStruct data)
        => new()
        {
            _name = (string)data["name"],
            _id = ToInt(data, "id"),
            _details = (string)data["details"],
            _parentId = ToInt(data, "parent_id"),
            _nodeTypeId = ToInt(data, "node_type_id"),
            _nodeOrder = ToInt(data, "node_order"),
        };

    /// <summary>
    /// </summary>
    /// <param name="data"></param>
    internal static Model.TestPlatform ToTestPlatform(XmlRpcStruct data)
        => new()
        {
            id = ToInt(data, "id"),
            name = (string)data["name"],
            notes = (string)data["notes"]
        };

    static int ToInt(XmlRpcStruct data, string name)
    {
        if (!data.ContainsKey(name))
        {
            return 0;
        }

        var val = data[name];
        switch (val)
        {
            case string stringValue:
                if (int.TryParse(stringValue, out var x))
                {
                    return x;
                }

                break;
            case int intValue:
                return intValue;
        }

        return 0;
    }

    static bool? ToBool(XmlRpcStruct data, string name)
    {
        if (!data.ContainsKey(name))
        {
            return null;
        }

        var val = data[name];
        if (val is not string stringValue)
        {
            return data[name] as bool?;
        }

        bool.TryParse(stringValue, out var result);
        return result;
    }

    static DateTime ToDate(XmlRpcStruct data, string name)
    {
        if (data.ContainsKey(name) && DateTime.TryParse((string)data[name], out var n))
        {
            return n;
        }

        return DateTime.MinValue;
    }

    static char ToChar(XmlRpcStruct data, string name)
    {
        if (!data.ContainsKey(name) || data[name] is not string)
        {
            return '\x00';
        }

        var s = (string)data[name];
        return s[0];
    }
}