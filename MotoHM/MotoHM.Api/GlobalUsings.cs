// MotoHM.Api/GlobalUsings.cs
// Layihə köküne qoy (MotoHM.Api.csproj ilə eyni qovluq).

// ---------- .NET / ASP.NET Core ----------
// Qeyd: csproj-da <ImplicitUsings>enable</ImplicitUsings> varsa, aşağıdakı
// "System*" və "Microsoft.AspNetCore.*" əsas sətirlər artıq avtomatik gəlir.
// Təkrar olması xəta vermir, sadəcə xəbərdarlıq (CS8933) çıxa bilər.
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Net;
global using System.Net.Http;
global using System.Net.Http.Json;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Security.Claims;
global using System.IdentityModel.Tokens.Jwt;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Net.Mail;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Routing;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using Microsoft.IdentityModel.Tokens;
global using Microsoft.OpenApi.Models;

// ---------- Verilənlər bazası ----------
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.ChangeTracking;
global using Microsoft.EntityFrameworkCore.Diagnostics;   // BackupWriteInterceptor (SaveChangesInterceptor)
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Dapper;
global using Microsoft.Data.SqlClient;
global using Npgsql;

// ---------- Üçüncü tərəf paketlər ----------
global using MediatR;
global using FluentValidation;
global using FluentValidation.Results;
global using Serilog.Events;
global using BCryptNet = BCrypt.Net.BCrypt;   // BCrypt.Net-Next; istifadə: BCryptNet.HashPassword(...)
global using Swashbuckle.AspNetCore.SwaggerGen;

// Newtonsoft.Json istifadə edirsənsə aç (System.Text.Json ilə ad toqquşması ola bilər):
// global using Newtonsoft.Json;

// ---------- Layihənin öz ümumi namespace-ləri ----------
// Loglardan gördüklərim: MotoHM.Api.Data, Data.Interceptors, Shared.Behaviors,
// Shared.Exceptions.Common, Shared.Email, Entites (yazılış belədir).
global using MotoHM.Api.Data;
global using MotoHM.Api.Data.Interceptors;
global using MotoHM.Api.Entites;
global using MotoHM.Api.Shared.Behaviors;
global using MotoHM.Api.Shared.Email;
global using MotoHM.Api.Shared.Exceptions.Common;

// DİQQƏT: MotoHM.Api.Modules.* namespace-lərini bura QOYMA.
// Hər modulda (Users.Login, Admin.ResyncBackup və s.) eyni adlı Handler / Command /
// Validator tipləri var, global olsalar "ambiguous reference" xətası çıxar.
