using System.Text.Json;

namespace Inventory.Web.Helpers
{
    public static class SessionHelper
    {
        private const string TokenKey = "JwtToken";
        private const string UserDataKey = "UserData";

        /// <summary>
        /// Guardar el token JWT en la sesión
        /// </summary>
        public static void SetToken(ISession session, string token)
        {
            session.SetString(TokenKey, token);
        }

        /// <summary>
        /// Obtener el token JWT de la sesión
        /// </summary>
        public static string? GetToken(ISession session)
        {
            return session.GetString(TokenKey);
        }

        /// <summary>
        /// Guardar datos del usuario en la sesión
        /// </summary>
        public static void SetUserData(ISession session, object userData)
        {
            var json = JsonSerializer.Serialize(userData);
            session.SetString(UserDataKey, json);
        }

        /// <summary>
        /// Obtener datos del usuario de la sesión
        /// </summary>
        public static T? GetUserData<T>(ISession session)
        {
            var json = session.GetString(UserDataKey);
            return json == null ? default : JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// Verificar si el usuario está autenticado
        /// </summary>
        public static bool IsAuthenticated(ISession session)
        {
            return !string.IsNullOrEmpty(GetToken(session));
        }

        /// <summary>
        /// Limpiar la sesión (logout)
        /// </summary>
        public static void ClearSession(ISession session)
        {
            session.Clear();
        }
    }
}

