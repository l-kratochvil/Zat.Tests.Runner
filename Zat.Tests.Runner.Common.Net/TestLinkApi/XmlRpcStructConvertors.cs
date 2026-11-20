namespace Zat.Tests.Runner.Common.Net.TestLinkApi;

using CookComputing.XmlRpc;

using Zat.Tests.Runner.Common.Net.TestLinkApi.Model;

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
            FileName: ToString(data, "file_name"),
            FileType: ToString(data, "file_type"),
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
            IsOpen: ToInt(data, "is_open") == 1,
            Name: ToString(data, "name"),
            Notes: ToString(data, "notes"),
            TestplanId: ToInt(data, "testplan_id"));

    public static TestCaseFromTestPlan ToTestCaseFromTestPlan(XmlRpcStruct data)
        => new(
            TestCaseId: ToInt(data, "tcase_id"),
            FullExternalId: ToString(data, "full_external_id"),
            Name: ToString(data, "tcase_name"));

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

    private static string ToString(XmlRpcStruct data, string name)
        => data.ContainsKey(name) ? (string?)data[name] ?? string.Empty : string.Empty;
}