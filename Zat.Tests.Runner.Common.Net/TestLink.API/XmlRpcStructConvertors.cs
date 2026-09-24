namespace Zat.Tests.Runner.Common.Net.TestLink.API;

using CookComputing.XmlRpc;

using Zat.Tests.Runner.Common.Net.TestLink.API.Model;

internal static class XmlRpcStructConvertors
{
    public static TestLinkErrorMessage ToTestLinkErrorMessage(XmlRpcStruct data)
        => new(
            Code: ToInt(data, "code"),
            Message: ToString(data, "message"));

    public static TestCase ToTestCase(XmlRpcStruct data)
        => new(
            Id: ToInt(data, "testcase_id"),
            Name: ToString(data, "name"),
            ExternalId: ToInt(data, "tc_external_id"),
            TestSuiteId: ToInt(data, "testsuite_id"));

    public static GeneralResult ToGeneralResult(XmlRpcStruct data)
    {
        var additionalInfo = data.ContainsKey("additionalInfo") && data["additionalInfo"] is XmlRpcStruct additionalInfoData
            ? ToAdditionalInfo(additionalInfoData)
            : null;

        return new GeneralResult(
            AdditionalInfo: additionalInfo,
            Id: ToInt(data, "id"),
            Message: ToString(data, "message"),
            Operation: ToString(data, "operation"),
            Status: (bool)data["status"]);
    }

    /// <summary>
    ///  constructor used by XMLRPC interface on decoding the function return
    /// </summary>
    /// <param name="data">data returned by Testlink</param>
    public static AttachmentRequestResponse ToAttachmentRequestResponse(XmlRpcStruct data)
        => new(
            Description: ToString(data, "description"),
            File_name: ToString(data, "file_name"),
            File_type: ToString(data, "file_type"),
            ForeignKeyId: ToInt(data, "fk_id"),
            LinkedTableName: ToString(data, "fk_table"),
            Size: ToInt(data, "file_size"),
            Title: ToString(data, "title"));

    /// <summary>
    ///  constructor used by XMLRPC interface on decoding the function return
    /// </summary>
    /// <param name="data">data returned by Testlink</param>
    public static AdditionalInfo ToAdditionalInfo(XmlRpcStruct data)
        => new(
            ExternalId: ToInt(data, "external_id"),
            HasDuplicate: ToBool(data, "has_duplicate"),
            Id: ToInt(data, "id"),
            Msg: ToString(data, "msg"),
            NewName: ToString(data, "new_name"),
            StatusOk: ToInt(data, "status_ok") == 1,
            VersionNumber: ToInt(data, "version_number"));

    public static Build ToBuild(XmlRpcStruct data)
        => new(
            Active: ToInt(data, "active") == 1,
            Id: ToInt(data, "id"),
            Is_open: ToInt(data, "is_open") == 1,
            Name: ToString(data, "name"),
            Notes: ToString(data, "notes"),
            Testplan_id: ToInt(data, "testplan_id"));

    public static TestCaseFromTestSuite ToTestCaseFromTestSuite(XmlRpcStruct data)
    {
        var details = data.ContainsKey("details")
            ? ToString(data, "details")
            : string.Empty;

        return new TestCaseFromTestSuite(
            Active: int.Parse((string)data["active"]) == 1,
            Author_id: ToInt(data, "author_id"),
            Creation_ts: ToDate(data, "creation_ts"),
            Details: details,
            Execution_type: ToInt(data, "execution_type"),
            External_id: ToString(data, "tc_external_id"),
            Id: ToInt(data, "id"),
            Importance: ToInt(data, "importance"),
            Is_open: int.Parse((string)data["is_open"]) == 1,
            Layout: ToString(data, "layout"),
            Modification_ts: ToDate(data, "modification_ts"),
            Name: ToString(data, "name"),
            Node_order: ToInt(data, "node_order"),
            Node_table: ToString(data, "node_table"),
            Node_type_id: ToInt(data, "node_type_id"),
            Parent_id: ToInt(data, "parent_id"),
            Preconditions: ToString(data, "preconditions"),
            Status: ToInt(data, "status"),
            Summary: ToString(data, "summary"),
            Tcversion_id: ToInt(data, "tcversion_id"),
            TestSuite_id: ToInt(data, "parent_id"),
            Updater_id: ToInt(data, "updater_id"),
            Version: ToInt(data, "version"));
    }

    /// <summary>
    ///  constructor used by the XML Rpc return
    /// </summary>
    /// <param name="data"></param>
    public static TestStep ToTestStep(XmlRpcStruct data)
        => new(
            Actions: ToString(data, "actions"),
            Active: ToInt(data, "active") == 1,
            Execution_type: ToInt(data, "execution_type"),
            Expected_results: ToString(data, "expected_results"),
            Id: ToInt(data, "id"),
            Step_number: ToInt(data, "step_number"));

    /// <summary>
    ///  constructor used by XMLRPC interface on decoding the function return
    /// </summary>
    /// <param name="data">data returned by Testlink</param>
    public static TestSuite ToTestSuite(XmlRpcStruct data)
        => new(
            Id: ToInt(data, "id"),
            Name: ToString(data, "name"),
            Details: ToString(data, "details"),
            NodeOrder: ToInt(data, "node_order"),
            NodeTypeId: ToInt(data, "node_type_id"),
            ParentId: ToInt(data, "parent_id"));

    /// <summary>
    /// </summary>
    /// <param name="data"></param>
    public static TestPlatform ToTestPlatform(XmlRpcStruct data)
        => new(
            Id: ToInt(data, "id"),
            Name: ToString(data, "name"),
            Notes: ToString(data, "notes"));

    private static int ToInt(XmlRpcStruct data, string name)
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

    private static bool? ToBool(XmlRpcStruct data, string name)
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

    private static DateTime ToDate(XmlRpcStruct data, string name)
    {
        if (data.ContainsKey(name) && DateTime.TryParse((string)data[name], out var n))
        {
            return n;
        }

        return DateTime.MinValue;
    }

    private static string ToString(XmlRpcStruct data, string name)
        => data.ContainsKey(name) ? (string?)data[name] ?? string.Empty : string.Empty;

    private static char ToChar(XmlRpcStruct data, string name)
    {
        if (!data.ContainsKey(name) || data[name] is not string)
        {
            return '\x00';
        }

        var s = (string)data[name];
        return s[0];
    }
}