var DataTypes = require("sequelize").DataTypes;
var _Cities = require("./Cities");
var _CitySites = require("./CitySites");
var _Notifications = require("./Notifications");
var _Predictions = require("./Predictions");
var _Ratings = require("./Ratings");
var _Sites = require("./Sites");
var _Timeslots = require("./Timeslots");
var _UserNotifications = require("./UserNotifications");
var _UserSiteCities = require("./UserSiteCities");
var _Users = require("./Users");
var ___EFMigrationsHistory = require("./__EFMigrationsHistory");

function initModels(sequelize) {
  var Cities = _Cities(sequelize, DataTypes);
  var CitySites = _CitySites(sequelize, DataTypes);
  var Notifications = _Notifications(sequelize, DataTypes);
  var Predictions = _Predictions(sequelize, DataTypes);
  var Ratings = _Ratings(sequelize, DataTypes);
  var Sites = _Sites(sequelize, DataTypes);
  var Timeslots = _Timeslots(sequelize, DataTypes);
  var UserNotifications = _UserNotifications(sequelize, DataTypes);
  var UserSiteCities = _UserSiteCities(sequelize, DataTypes);
  var Users = _Users(sequelize, DataTypes);
  var __EFMigrationsHistory = ___EFMigrationsHistory(sequelize, DataTypes);

  Cities.belongsToMany(Sites, { as: 'Site_Id_Sites', through: CitySites, foreignKey: "City_Id", otherKey: "Site_Id" });
  Sites.belongsToMany(Cities, { as: 'City_Id_Cities', through: CitySites, foreignKey: "Site_Id", otherKey: "City_Id" });
  CitySites.belongsTo(Cities, { as: "City", foreignKey: "City_Id"});
  Cities.hasMany(CitySites, { as: "CitySites", foreignKey: "City_Id"});
  Predictions.belongsTo(CitySites, { as: "City", foreignKey: "City_Id"});
  CitySites.hasMany(Predictions, { as: "Predictions", foreignKey: "City_Id"});
  Predictions.belongsTo(CitySites, { as: "Site", foreignKey: "Site_Id"});
  CitySites.hasMany(Predictions, { as: "Site_Predictions", foreignKey: "Site_Id"});
  UserSiteCities.belongsTo(CitySites, { as: "City", foreignKey: "City_Id"});
  CitySites.hasMany(UserSiteCities, { as: "UserSiteCities", foreignKey: "City_Id"});
  UserSiteCities.belongsTo(CitySites, { as: "Site", foreignKey: "Site_Id"});
  CitySites.hasMany(UserSiteCities, { as: "Site_UserSiteCities", foreignKey: "Site_Id"});
  UserNotifications.belongsTo(Notifications, { as: "City", foreignKey: "City_Id"});
  Notifications.hasMany(UserNotifications, { as: "UserNotifications", foreignKey: "City_Id"});
  UserNotifications.belongsTo(Notifications, { as: "Site", foreignKey: "Site_Id"});
  Notifications.hasMany(UserNotifications, { as: "Site_UserNotifications", foreignKey: "Site_Id"});
  UserNotifications.belongsTo(Notifications, { as: "Timeslot", foreignKey: "Timeslot_Id"});
  Notifications.hasMany(UserNotifications, { as: "Timeslot_UserNotifications", foreignKey: "Timeslot_Id"});
  Notifications.belongsTo(Predictions, { as: "City", foreignKey: "City_Id"});
  Predictions.hasMany(Notifications, { as: "Notifications", foreignKey: "City_Id"});
  Notifications.belongsTo(Predictions, { as: "Site", foreignKey: "Site_Id"});
  Predictions.hasMany(Notifications, { as: "Site_Notifications", foreignKey: "Site_Id"});
  Notifications.belongsTo(Predictions, { as: "Timeslot", foreignKey: "Timeslot_Id"});
  Predictions.hasMany(Notifications, { as: "Timeslot_Notifications", foreignKey: "Timeslot_Id"});
  Ratings.belongsTo(Predictions, { as: "City", foreignKey: "City_Id"});
  Predictions.hasMany(Ratings, { as: "Ratings", foreignKey: "City_Id"});
  Ratings.belongsTo(Predictions, { as: "Site", foreignKey: "Site_Id"});
  Predictions.hasMany(Ratings, { as: "Site_Ratings", foreignKey: "Site_Id"});
  Ratings.belongsTo(Predictions, { as: "Timeslot", foreignKey: "Timeslot_Id"});
  Predictions.hasMany(Ratings, { as: "Timeslot_Ratings", foreignKey: "Timeslot_Id"});
  CitySites.belongsTo(Sites, { as: "Site", foreignKey: "Site_Id"});
  Sites.hasMany(CitySites, { as: "CitySites", foreignKey: "Site_Id"});
  Predictions.belongsTo(Timeslots, { as: "Timeslot", foreignKey: "Timeslot_Id"});
  Timeslots.hasMany(Predictions, { as: "Predictions", foreignKey: "Timeslot_Id"});
  Ratings.belongsTo(Users, { as: "Email_User", foreignKey: "Email"});
  Users.hasMany(Ratings, { as: "Ratings", foreignKey: "Email"});
  UserNotifications.belongsTo(Users, { as: "Email_User", foreignKey: "Email"});
  Users.hasMany(UserNotifications, { as: "UserNotifications", foreignKey: "Email"});
  UserSiteCities.belongsTo(Users, { as: "Email_User", foreignKey: "Email"});
  Users.hasMany(UserSiteCities, { as: "UserSiteCities", foreignKey: "Email"});

  return {
    Cities,
    CitySites,
    Notifications,
    Predictions,
    Ratings,
    Sites,
    Timeslots,
    UserNotifications,
    UserSiteCities,
    Users,
    __EFMigrationsHistory,
  };
}
module.exports = initModels;
module.exports.initModels = initModels;
module.exports.default = initModels;
