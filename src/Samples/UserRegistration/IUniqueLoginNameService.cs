namespace UserRegistration
{
    public interface IUniqueLoginNameService
    {
        /// <summary>
        /// true表示验证通过
        /// </summary>
        bool Validate(string loginName, string correlationId);
    }
}
