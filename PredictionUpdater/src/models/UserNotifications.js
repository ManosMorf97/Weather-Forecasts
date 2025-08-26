const Sequelize = require('sequelize');
module.exports = function(sequelize, DataTypes) {
  return sequelize.define('UserNotifications', {
    Email: {
      type: DataTypes.STRING(450),
      allowNull: false,
      primaryKey: true,
      references: {
        model: 'Users',
        key: 'Email'
      }
    },
    City_Id: {
      type: DataTypes.INTEGER,
      allowNull: false,
      primaryKey: true,
      references: {
        model: 'Notifications',
        key: 'Timeslot_Id'
      }
    },
    Site_Id: {
      type: DataTypes.INTEGER,
      allowNull: false,
      primaryKey: true,
      references: {
        model: 'Notifications',
        key: 'Timeslot_Id'
      }
    },
    Timeslot_Id: {
      type: DataTypes.INTEGER,
      allowNull: false,
      primaryKey: true,
      references: {
        model: 'Notifications',
        key: 'Timeslot_Id'
      }
    },
    IsRead: {
      type: DataTypes.BOOLEAN,
      allowNull: false
    }
  }, {
    sequelize,
    tableName: 'UserNotifications',
    schema: 'dbo',
    timestamps: false,
    indexes: [
      {
        name: "IX_UserNotifications_Site_Id_City_Id_Timeslot_Id",
        fields: [
          { name: "Site_Id" },
          { name: "City_Id" },
          { name: "Timeslot_Id" },
        ]
      },
      {
        name: "PK_UserNotifications",
        unique: true,
        fields: [
          { name: "Email" },
          { name: "City_Id" },
          { name: "Site_Id" },
          { name: "Timeslot_Id" },
        ]
      },
    ]
  });
};
